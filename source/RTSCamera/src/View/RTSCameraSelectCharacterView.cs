using MissionSharedLibrary.Utilities;
using RTSCamera.ActorComponents;
using RTSCamera.Config.HotKey;
using RTSCameraAgentComponent;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace RTSCamera.View
{
    public class RTSCameraSelectCharacterView : MissionView
    {
        private bool _isSelectingCharacter;
        private Agent _mouseOverAgent;
        private Agent _selectedAgent;
        private static readonly uint MouseOverColor = new Color(0.3f, 1.0f, 1.0f).ToUnsignedInteger();
        private static readonly uint SelectedColor = new Color(0.2f, 0.5f, 1.0f).ToUnsignedInteger();
        private static readonly uint EnemyMouseOverColor = new Color(0.98f, 0.6f, 0.5f).ToUnsignedInteger();
        private static readonly uint EnemySelectedColor = new Color(0.98f, 0.2f, 0.3f).ToUnsignedInteger();
        private GauntletLayer _gauntletLayer;
        private SelectCharacterVM _dataSource;
        private FlyCameraMissionView _flyCameraMissionView;

        public bool IsSelectingCharacter
        {
            get => _isSelectingCharacter;
            set
            {
                if (_isSelectingCharacter == value)
                    return;
                _isSelectingCharacter = value;
                if (_isSelectingCharacter)
                {
                    GameTexts.SetVariable("KeyName",RTSCameraGameKeyCategory.GetKey(GameKeyEnum.ControlTroop).ToSequenceString());
                    Activate();
                }
                else
                {
                    Deactivate();
                    MouseOverAgent = null;
                    SelectedAgent = null;
                }
            }
        }

        public Agent MouseOverAgent
        {
            get => _mouseOverAgent;
            set
            {
                if (_mouseOverAgent == value)
                    return;
                _mouseOverAgent?.GetComponent<ColouredOutlineComponent>()?.SetContourColor((int)ColorLevel.MouseOverAgent, null, false, true);
                _mouseOverAgent = value;
                SetMouseOver(value);
            }
        }

        public Agent SelectedAgent
        {
            get => _selectedAgent;
            set
            {
                if (_selectedAgent == value)
                    return;
                _selectedAgent?.GetComponent<ColouredOutlineComponent>()?.SetContourColor((int)ColorLevel.SelectedAgent, null, false, true);
                _selectedAgent = value;
                SetSelected(value);
            }
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();

            ViewOrderPriority = 23;
            _flyCameraMissionView = Mission.GetMissionBehavior<FlyCameraMissionView>();
            MissionLibrary.Event.MissionEvent.PostSwitchTeam += OnPostSwitchTeam;
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();

            Deactivate();
            _mouseOverAgent = null;
            _selectedAgent = null;
            MissionLibrary.Event.MissionEvent.PostSwitchTeam -= OnPostSwitchTeam;
        }

        public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnEarlyAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            affectedAgent.GetComponent<ColouredOutlineComponent>()?.ClearContourColor();
        }

        public bool LockOnAgent(Agent candidate)
        {
            if (Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter)
                return false;
            var agentToLock = SelectedAgent ?? candidate;
            if ((SelectedAgent != null && IsSelectingCharacter) || candidate != null && MissionScreen.LastFollowedAgent != agentToLock)
            {
                _flyCameraMissionView.FocusOnAgent(agentToLock);
                IsSelectingCharacter = false;
                return true;
            }

            return false;
        }

        private void Activate()
        {
            _gauntletLayer = new GauntletLayer(ViewOrderPriority) { IsFocusLayer = false };
            _dataSource = new SelectCharacterVM();
            _gauntletLayer.LoadMovie(nameof(RTSCameraSelectCharacterView), _dataSource);
            _gauntletLayer.InputRestrictions.SetInputRestrictions();
            MissionScreen.AddLayer(_gauntletLayer);
        }

        private void Deactivate()
        {
            _dataSource?.OnFinalize();
            _dataSource = null;
            if (_gauntletLayer != null)
            {
                _gauntletLayer.InputRestrictions.ResetInputRestrictions();
                MissionScreen.RemoveLayer(_gauntletLayer);
                _gauntletLayer = null;
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);

            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SelectCharacter).IsKeyPressed(Input) ||
                IsSelectingCharacter && _gauntletLayer.Input.IsKeyPressed(InputKey.RightMouseButton))
            {
                IsSelectingCharacter = !IsSelectingCharacter;
            }

            if (Mission.Mode == MissionMode.Conversation || Mission.Mode == MissionMode.Barter)
                return;

            if (IsSelectingCharacter)
            {
                UpdateMouseOverCharacter();
            }

            if (_gauntletLayer != null)
            {
                if (_gauntletLayer.Input.IsKeyPressed(InputKey.LeftMouseButton))
                {
                    SelectedAgent = MouseOverAgent;
                }
            }
        }

        public override bool OnEscape()
        {
            if (IsSelectingCharacter)
            {
                IsSelectingCharacter = false;
                return true;
            }

            return base.OnEscape();
        }

        private void OnPostSwitchTeam()
        {
            SetMouseOver(MouseOverAgent);
            SetSelected(SelectedAgent);
        }

        private void SetMouseOver(Agent agent)
        {
            agent?.GetComponent<ColouredOutlineComponent>()?.SetContourColor((int)ColorLevel.MouseOverAgent, Utility.IsEnemy(agent) ? EnemyMouseOverColor : MouseOverColor, true, true);
        }

        private void SetSelected(Agent agent)
        {
            agent?.GetComponent<ColouredOutlineComponent>()?.SetContourColor((int)ColorLevel.SelectedAgent, Utility.IsEnemy(agent) ? EnemySelectedColor : SelectedColor, true, true);
        }

        private void UpdateMouseOverCharacter()
        {
            MissionScreen.ScreenPointToWorldRay(Input.GetMousePositionRanged(), out var rayBegin, out var rayEnd);
            var agent = Mission.RayCastForClosestAgent(rayBegin, rayEnd, out var distance, -1, 0.1f);
            if (agent != null && agent.IsMount)
                agent = agent.RiderAgent ?? null;

            MouseOverAgent = agent;
            //if (MouseOverAgent != null && Mission.MainAgent != null)
            //{
            //    Utility.DisplayMessage((MouseOverAgent.Position - Mission.MainAgent.Position).Length
            //        .ToString());
            //}
        }
    }
}
