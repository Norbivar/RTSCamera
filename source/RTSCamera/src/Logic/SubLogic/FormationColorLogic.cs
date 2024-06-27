using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using RTSCamera.Logic;
using RTSCameraAgentComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;

namespace RTSCamera
{
    public class FormationColorLogic
    {
        private static readonly OrderType[] movementOrderTypes =
        {
            OrderType.Move,
            OrderType.MoveToLineSegment,
            OrderType.MoveToLineSegmentWithHorizontalLayout,
            OrderType.Charge,
            OrderType.ChargeWithTarget,
            OrderType.StandYourGround,
            OrderType.FollowMe,
            OrderType.FollowEntity,
            OrderType.GuardMe,
            OrderType.Retreat,
            OrderType.AdvanceTenPaces,
            OrderType.FallBackTenPaces,
            OrderType.Advance,
            OrderType.FallBack
        };

        private readonly uint _allySelectedColor = new Color(0.2f, 1.0f, 0.2f).ToUnsignedInteger();
        private readonly uint _allyTargetColor = new Color(0.3f, 0.3f, 1.0f).ToUnsignedInteger(); // maybe unused -> we cannot "target" allies?
        private readonly uint _mouseOverAllyColor = new Color(0.3f, 1.0f, 1.0f).ToUnsignedInteger();
        private readonly uint _enemySelectedColor = new Color(1.0f, 0.5f, 0.5f).ToUnsignedInteger(); // maybe unused -> we cannot "select" enemeis?
        private readonly uint _enemyTargetColor = new Color(1.0f, 0.2f, 0.2f).ToUnsignedInteger();
        private readonly uint _mouseOverEnemyColor = new Color(0.8f, 0.6f, 0.6f).ToUnsignedInteger();
        private readonly List<Formation> _enemyAsTargetFormations = new List<Formation>();
        private readonly List<Formation> _allyAsTargetFormations = new List<Formation>();
        private readonly List<Formation> _allySelectedFormations = new List<Formation>();
        private readonly List<Formation> _temporarilyUpdatedFormations = new List<Formation>();
        private OrderController PlayerOrderController => Mission.Current.PlayerTeam?.PlayerOrderController;
        private Formation _mouseOverFormation;
        private MissionGauntletSingleplayerOrderUIHandler _orderUiHandler;
        private readonly RTSCameraConfig _config = RTSCameraConfig.Get();

        private bool _isOrderShown;
        private bool HighlightEnabled => _isOrderShown;
        private bool HighlightEnabledForSelectedFormation => _isOrderShown && _config.HighlightOwnSelectedFormation;
        private bool HighlightEnabledForAsTargetFormation => _isOrderShown && _config.HighlightEnemyTargetFormation;

        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        private readonly RTSCameraLogic _logic;
        public Mission Mission => _logic.Mission;

        public FormationColorLogic(RTSCameraLogic logic)
        {
            _logic = logic;
        }

        public void OnBehaviourInitialize()
        {
            Mission.Current.Teams.OnPlayerTeamChanged += Mission_OnPlayerTeamChanged;
            Game.Current.EventManager.RegisterEvent<MissionPlayerToggledOrderViewEvent>(OnToggleOrderViewEvent);
            _orderUiHandler = Mission.Current.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
        }

        public  void OnRemoveBehaviour()
        {
            _actionQueue.Clear();
            _enemyAsTargetFormations.Clear();
            _allyAsTargetFormations.Clear();
            _allySelectedFormations.Clear();
            _temporarilyUpdatedFormations.Clear();
            Game.Current.EventManager.UnregisterEvent<MissionPlayerToggledOrderViewEvent>(OnToggleOrderViewEvent); ;
        }

        public void OnPreDisplayMissionTick(float dt)
        {
            try
            {
                bool noAction = _actionQueue.IsEmpty();
                while (!_actionQueue.IsEmpty())
                    _actionQueue.Dequeue()?.Invoke();

                var list = _temporarilyUpdatedFormations.GroupBy(formation => formation).Select(grouping => grouping.Key).ToList();
                var additionalFormationToUpdate = list.FirstOrDefault();

                if (!noAction)
                {
                    foreach (var group in _enemyAsTargetFormations.Concat(_allyAsTargetFormations)
                             .Concat(_allySelectedFormations).GroupBy(formation => formation))
                    {
                        if (group.Key != additionalFormationToUpdate)
                        {
                            group.Key?.ApplyActionOnEachUnit(a => a.GetComponent<RTSCameraComponent>()?.UpdateContour());
                        }
                    }
                }

                if (additionalFormationToUpdate != null)
                {
                    _temporarilyUpdatedFormations.RemoveAll(f => f == additionalFormationToUpdate);
                    additionalFormationToUpdate.ApplyActionOnEachUnit(a => a.GetComponent<RTSCameraComponent>()?.UpdateContour());
                }
            }
            catch (Exception e)
            {
                Utility.DisplayMessageForced(e.ToString());
            }
        }

        public  void AfterAddTeam(Team team)
        {
            team.OnOrderIssued += OnOrderIssued;
            team.OnFormationsChanged += OnFormationsChanged;
            team.PlayerOrderController.OnSelectedFormationsChanged += OrderController_OnSelectedFormationsChanged;
        }

        public  void OnAgentBuild(Agent agent, Banner banner)
        {
            if (agent.Formation != null)
            {
                bool isEnemy = Utility.IsEnemy(agent.Formation);
                if (agent.Formation == _mouseOverFormation)
                    SetAgentMouseOverContour(agent, isEnemy);

                if (isEnemy)
                {
                    if (_enemyAsTargetFormations.Contains(agent.Formation))
                        SetAgentAsTargetContour(agent, true);
                }
                else
                {
                    if (_allySelectedFormations.Contains(agent.Formation))
                        SetAgentSelectedContour(agent, false);
                    if (_allyAsTargetFormations.Contains(agent.Formation))
                        SetAgentAsTargetContour(agent, false);
                }
            }
        }

        public  void OnAgentFleeing(Agent affectedAgent)
        {
            ClearAgentFormationContour(affectedAgent);
            affectedAgent.GetComponent<RTSCameraComponent>()?.UpdateContour();
        }

        public void MouseOver(Formation formation)
        {
            if (formation == _mouseOverFormation)
                return;

            if (_mouseOverFormation != null)
                ClearFormationMouseOverContour(_mouseOverFormation);

            if (!HighlightEnabled)
                return;

            if (formation != null)
            {
                bool isEnemy = Utility.IsEnemy(formation);
                if (isEnemy ? HighlightEnabledForAsTargetFormation : HighlightEnabledForSelectedFormation)
                    SetFormationMouseOverContour(formation, Utility.IsEnemy(formation));
            }
        }

        public void SetEnableContourForSelectedFormation(bool enable)
        {
            if (HighlightEnabled)
                SetFocusContour();
            else
                ClearContour();
        }

        private void OnToggleOrderViewEvent(MissionPlayerToggledOrderViewEvent e)
        {
            _isOrderShown = e.IsOrderEnabled;
            if (HighlightEnabled)
            {
                SetFocusContour();
            }
            else
            {
                ClearContour();
            }
        }

        private void OnOrderIssued(OrderType orderType, MBReadOnlyList<Formation> appliedFormations, OrderController orderController, params object[] delegateParams)
        {
            if (!HighlightEnabledForAsTargetFormation || movementOrderTypes.FindIndex(o => o == orderType) == -1)
                return;

            if (!_allySelectedFormations.Intersect(appliedFormations).IsEmpty())
            {
                ClearEnemyFocusContour();
                SetFocusContour();
            }
        }

        private void OnFormationsChanged(Team team, Formation formation)
        {
            if (!HighlightEnabled)
                return;

            var mouseOverFormation = _mouseOverFormation;
            _mouseOverFormation = null;

            ClearFormationAllContour(formation);
            SetFocusContour();
            MouseOver(mouseOverFormation);
        }

        private void OrderController_OnSelectedFormationsChanged()
        {
            if (!HighlightEnabled)
                return;

            SetFocusContour();
            if (_orderUiHandler == null)
                return;

            foreach (OrderTroopItemVM troop in ((MissionOrderVM)typeof(MissionGauntletSingleplayerOrderUIHandler).GetField("_dataSource", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_orderUiHandler)).TroopController.TroopList)
            {
                troop.IsSelectable = PlayerOrderController.IsFormationSelectable(troop.Formation);
                troop.IsSelected = troop.IsSelectable && PlayerOrderController.IsFormationListening(troop.Formation);
            }
        }

        private void Mission_OnPlayerTeamChanged(Team arg1, Team arg2)
        {
            if (!HighlightEnabled)
                return;

            UpdateContour();
        }

        private void UpdateContour()
        {
            ClearContour();
            SetFocusContour();
        }

        private void SetFocusContour()
        {
            foreach (var formation in PlayerOrderController?.SelectedFormations ?? Enumerable.Empty<Formation>())
            {
                if (!_allySelectedFormations.Contains(formation))
                {
                    SetFormationSelectedContour(formation, false);
                }
            }
            foreach (var formation in _allySelectedFormations)
            {
                if (!PlayerOrderController?.SelectedFormations.Contains(formation) ?? true)
                {
                    ClearFormationFocusContour(formation);
                }
            }

            _allySelectedFormations.Clear();
            _allySelectedFormations.AddRange(PlayerOrderController?.SelectedFormations ?? Enumerable.Empty<Formation>());

            var enemyAsTargetFormations = PlayerOrderController?.SelectedFormations
                .Select(formation => formation.TargetFormation).Where(formation => formation != null).ToList() ?? new List<Formation>();

            foreach (var formation in enemyAsTargetFormations)
            {
                if (HighlightEnabledForAsTargetFormation && !_enemyAsTargetFormations.Contains(formation))
                    SetFormationAsTargetContour(formation, true);
            }
            foreach (var formation in _enemyAsTargetFormations)
            {
                if (!HighlightEnabledForAsTargetFormation || !enemyAsTargetFormations.Contains(formation))
                {
                    ClearFormationFocusContour(formation);
                }
            }

            _enemyAsTargetFormations.Clear();
            _enemyAsTargetFormations.AddRange(enemyAsTargetFormations);

            if (Utility.IsTeamValid(Mission.Current.PlayerEnemyTeam))
            {
                var allyAsTargetFormations = Mission.Current.PlayerEnemyTeam.FormationsIncludingSpecialAndEmpty // TODO: might not be correct
                    .Where(formation => formation.GetReadonlyMovementOrderReference().OrderType == OrderType.ChargeWithTarget)
                    .Select(formation => formation.GetReadonlyMovementOrderReference().TargetFormation).ToList();

                foreach (var formation in allyAsTargetFormations)
                {
                    if (HighlightEnabledForAsTargetFormation && !_allyAsTargetFormations.Contains(formation))
                        SetFormationAsTargetContour(formation, false);
                }
                foreach (var formation in _allyAsTargetFormations)
                {
                    if (!HighlightEnabledForAsTargetFormation || !allyAsTargetFormations.Contains(formation))
                    {
                        ClearFormationFocusContour(formation);
                    }
                }

                _allyAsTargetFormations.Clear();
                _allyAsTargetFormations.AddRange(allyAsTargetFormations);
            }
        }

        private void ClearContour()
        {
            foreach (var formation in _enemyAsTargetFormations)
            {
                ClearFormationAllContour(formation);
            }

            _enemyAsTargetFormations.Clear();

            foreach (var formation in _allySelectedFormations)
            {
                ClearFormationAllContour(formation);
            }

            _allySelectedFormations.Clear();

            foreach (var formation in _allyAsTargetFormations)
            {
                ClearFormationAllContour(formation);
            }

            _allyAsTargetFormations.Clear();

            if (_mouseOverFormation == null)
                return;
            ClearFormationAllContour(_mouseOverFormation);
            _mouseOverFormation = null;
        }

        private void ClearEnemyFocusContour()
        {
            foreach (var formation in _enemyAsTargetFormations)
            {
                ClearFormationFocusContour(formation);
            }

            _enemyAsTargetFormations.Clear();
        }

        private void ClearAllyFocusContour()
        {
            foreach (var formation in _allySelectedFormations)
            {
                ClearFormationFocusContour(formation);
            }

            _allySelectedFormations.Clear();

            foreach (var formation in _allyAsTargetFormations)
            {
                ClearFormationFocusContour(formation);
            }

            _allyAsTargetFormations.Clear();
        }

        private void SetFormationMouseOverContour(Formation formation, bool isEnemy)
        {
            _mouseOverFormation = formation;
            _actionQueue.Enqueue(() =>
            {
                formation.ApplyActionOnEachUnit(agent => SetAgentMouseOverContour(agent, isEnemy));
                _temporarilyUpdatedFormations.Add(formation);
            });
        }

        private void SetFormationAsTargetContour(Formation formation, bool isEnemy)
        {
            if (isEnemy)
                _enemyAsTargetFormations.Add(formation);
            else
                _allyAsTargetFormations.Add(formation);
            _actionQueue.Enqueue(() =>
            {
                formation.ApplyActionOnEachUnit(agent => SetAgentAsTargetContour(agent, isEnemy));
            });
        }

        private void SetFormationSelectedContour(Formation formation, bool isEnemy)
        {
            if (!isEnemy)
                _allySelectedFormations.Add(formation);
            if (HighlightEnabledForSelectedFormation)
                _actionQueue.Enqueue(() =>
                {
                    formation.ApplyActionOnEachUnit(agent => SetAgentSelectedContour(agent, isEnemy));
                });
        }

        private void SetAgentMouseOverContour(Agent agent, bool enemy)
        {
            agent.GetComponent<RTSCameraComponent>()?.SetContourColor((int)ColorLevel.MouseOverFormation,
                enemy ? _mouseOverEnemyColor : _mouseOverAllyColor, true, false);
        }

        private void SetAgentAsTargetContour(Agent agent, bool enemy)
        {
            agent.GetComponent<RTSCameraComponent>()?.SetContourColor((int)ColorLevel.TargetFormation,
                enemy ? _enemyTargetColor : _allyTargetColor, true, false);
        }

        private void SetAgentSelectedContour(Agent agent, bool enemy)
        {
            agent.GetComponent<RTSCameraComponent>()?.SetContourColor((int)ColorLevel.SelectedFormation,
                enemy ? _enemySelectedColor : _allySelectedColor, true, false);
        }

        private void ClearFormationMouseOverContour(Formation formation)
        {
            ClearFormationContour(formation, ColorLevel.MouseOverFormation);
            _mouseOverFormation = null;
        }

        private void ClearFormationFocusContour(Formation formation)
        {
            _actionQueue.Enqueue(() =>
            {
                formation.ApplyActionOnEachUnit(agent =>
                    agent.GetComponent<RTSCameraComponent>()?.ClearTargetOrSelectedFormationColor());
                _temporarilyUpdatedFormations.Add(formation);
            });
        }

        private void ClearFormationContour(Formation formation, ColorLevel level)
        {
            _actionQueue.Enqueue(() =>
            {
                formation.ApplyActionOnEachUnit(agent =>
                    agent.GetComponent<RTSCameraComponent>()?.SetContourColor((int)level, null, true, false));
                _temporarilyUpdatedFormations.Add(formation);
            });
        }

        private void ClearFormationAllContour(Formation formation)
        {
            _actionQueue.Enqueue(() =>
            {
                formation.ApplyActionOnEachUnit(ClearAgentFormationContour);
                _temporarilyUpdatedFormations.Add(formation);
            });
        }

        private static void ClearAgentFormationContour(Agent agent)
        {
            agent.GetComponent<RTSCameraComponent>()?.ClearFormationColor();
        }
    }
}
