using MissionSharedLibrary.Utilities;
using RTSCamera.CampaignGame.Behavior;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using System.Collections.Generic;
using System.ComponentModel;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RTSCamera.Logic.SubLogic
{
    public class SwitchFreeCameraLogic
    {
        private readonly RTSCameraLogic _logic;
        private readonly RTSCameraConfig _config = RTSCameraConfig.Get();

        private ControlTroopLogic _controlTroopLogic;

        private bool _isFirstTimeMainAgentChanged = true;
        private bool _isDeploymentFinishing = false;
        private FormationClass _formationClassInDeployment;
        private bool _switchToFreeCameraAfter100ms;
        private float _switchToFreeCameraTimer;
        private List<FormationClass> _playerFormations;
        private float _updatePlayerFormationTime;

        public Mission Mission => _logic.Mission;

        public List<FormationClass> PlayerFormations => _playerFormations ??= new List<FormationClass>();

        public FormationClass CurrentPlayerFormation
        {
            get => Mission.PlayerTeam?.TeamIndex < PlayerFormations.Count
                ? PlayerFormations[Mission.PlayerTeam.TeamIndex]
                : (FormationClass)_config.PlayerFormation;
            set
            {
                if (Mission.PlayerTeam?.TeamIndex < PlayerFormations.Count)
                    PlayerFormations[Mission.PlayerTeam.TeamIndex] = value;
            }
        }

        public bool IsSpectatorCamera;

        public SwitchFreeCameraLogic(RTSCameraLogic logic)
        {
            _logic = logic;
        }

        public void OnBehaviourInitialize()
        {
            _controlTroopLogic = _logic.ControlTroopLogic;

            Mission.OnMainAgentChanged += OnMainAgentChanged;
            MissionLibrary.Event.MissionEvent.ToggleFreeCamera += ToggleAutomaticSlowMotionMode;
		}

        public void AfterAddTeam(Team team)
        {
            PlayerFormations.Add((FormationClass)_config.PlayerFormation);
        }

        public void OnRemoveBehaviour()
        {
            Mission.OnMainAgentChanged -= OnMainAgentChanged;
			MissionLibrary.Event.MissionEvent.ToggleFreeCamera -= ToggleAutomaticSlowMotionMode;
			WatchBattleBehavior.WatchMode = false;
        }

        public void OnTeamDeployed(Team team)
        {
            if (team == Mission.PlayerTeam)
            {
                if (WatchBattleBehavior.WatchMode && Mission.MainAgent == null)
                {
                    _controlTroopLogic.SetMainAgent();
                    if (Mission.MainAgent != null)
                    {
                        Utility.SetIsPlayerAgentAdded(_controlTroopLogic.MissionScreen, true);
                        if (Mission.PlayerTeam.IsPlayerGeneral)
                            Utility.SetPlayerAsCommander(true);
                        team.PlayerOrderController?.SelectAllFormations();
                    }
                }
            }
        }

        public void OnMissionTick(float dt)
        {
            if (_switchToFreeCameraAfter100ms)
            {
                _switchToFreeCameraTimer += dt;
                if (_switchToFreeCameraTimer > 0.1)
                {
                    _switchToFreeCameraAfter100ms = false;
                    _switchToFreeCameraTimer = 0;
                    SwitchToFreeCamera();
                }
            }

            _updatePlayerFormationTime += dt;
            if (_updatePlayerFormationTime > 0.1f && !Utility.IsPlayerDead() &&
                Mission.MainAgent.Formation != null)
            {
                _updatePlayerFormationTime = 0;
                CurrentPlayerFormation = Mission.MainAgent.Formation.FormationIndex;
            }

            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.FreeCamera).IsKeyPressed(Mission.InputManager))
            {
                SwitchCamera();
            }
        }

        public void SwitchCamera()
        {
            if (IsSpectatorCamera)
            {
                SwitchToAgent();
            }
            else
            {
                SwitchToFreeCamera();
            }
        }

        public void OnAgentControllerChanged(Agent agent)
        {
            if (agent.Controller == Agent.ControllerType.Player || agent.Controller == Agent.ControllerType.None)
            {
                agent.SetMaximumSpeedLimit(-1, false);
                agent.MountAgent?.SetMaximumSpeedLimit(-1, false);
                //agent.StopRetreating();
                TrySetPlayerFormation();

                // If MainAgent.Controller = Controller.Player is called from DeploymentMissionController, then we will try set the player formation i then reset _isDeploymentFinishing.
                _isDeploymentFinishing = false;
                if (agent.Formation == null)
                    return;

                CurrentPlayerFormation = agent.Formation.FormationIndex;
            }
            else if (agent == Mission.MainAgent)
            {
                Utility.SetHasPlayer(agent.Formation, false);
                TrySetPlayerFormation();

                if (agent.Formation == null)
                    return;

                CurrentPlayerFormation = agent.Formation.FormationIndex;
            }
        }

        public void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            if (oldMissionMode == MissionMode.Deployment && Mission.Mode == MissionMode.Battle)
            {
                // CurrentPlayerFormation will be changed when player agent is set to Player controller because OnMainAgentChanged will be called.
                // So we need to cache it here.
                _formationClassInDeployment = CurrentPlayerFormation;
                TrySetPlayerFormation(true);
                // OnMissionModeChange is called before the player is promoted to general formation in DeploymentMissionController.
                // So we need to still set player formation when player controller is set to Player in DeploymentMissionController later.
                _isDeploymentFinishing = true;
            }
        }

        private void TrySetPlayerFormation(bool isDeploymentFinishing = false)
        {
            // In watch mode, after deployment stage the player formation needs to be reset from General formation.
            if (WatchBattleBehavior.WatchMode)
            {
                if (_isDeploymentFinishing || isDeploymentFinishing)
                {
                    Utility.SetMainAgentFormationClass(_formationClassInDeployment);
                }
            }
        }

        private void OnMainAgentChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Mission.MainAgent != null)
            {
                if (Mission.Mode == MissionMode.Battle || Mission.Mode == MissionMode.Deployment)
                {
                    if (_isFirstTimeMainAgentChanged)
                    {
                        // try to switch to free camera by default.
                        _isFirstTimeMainAgentChanged = false;
                        if (_config.UseFreeCameraByDefault || WatchBattleBehavior.WatchMode)
                        {
                            _switchToFreeCameraAfter100ms = true;
                            _switchToFreeCameraTimer = 0;
                        }
                    }
                    if (Mission.MainAgent.Formation != null)
                        CurrentPlayerFormation = Mission.MainAgent.Formation.FormationIndex;

                    if (IsSpectatorCamera || WatchBattleBehavior.WatchMode)
                    {
                        UpdateMainAgentControllerInFreeCamera();
                    }
                    else
                    {
                        if (Mission.MainAgent.Controller != Agent.ControllerType.Player)
                            _controlTroopLogic.ControlMainAgent(false);
                    }
                }
            }
            else if (IsSpectatorCamera || (_config.ControlAllyAfterDeath && !Mission.IsFastForward))
            {
                _controlTroopLogic.SetMainAgent();
            }
        }

        private void UpdateMainAgentControllerInFreeCamera()
        {
            Utilities.Utility.UpdateMainAgentControllerInFreeCamera(Mission.MainAgent, _config.GetPlayerControllerInFreeCamera());
            Utilities.Utility.UpdateMainAgentControllerState(Mission.MainAgent, IsSpectatorCamera,
                _config.GetPlayerControllerInFreeCamera());
        }

        public void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectedAgent == null)
            {
                return;
            }
            if (Mission.MainAgent == affectedAgent)
            {
                if (_config.ControlAllyAfterDeath || IsSpectatorCamera)
                {
                    if (Utilities.Utility.IsBattleCombat(Mission) && Mission.MainAgent.Character == CharacterObject.PlayerCharacter)
                        Utility.DisplayLocalizedText("str_rts_camera_player_dead", null, new Color(1, 0, 0));

                    // mask code in Mission.OnAgentRemoved so that formations will not be delegated to AI after player dead.
                    if (_controlTroopLogic.GetAgentToControl() != null)
                    {
                        affectedAgent.OnMainAgentWieldedItemChange = null;
                        bool shouldSmoothToAgent = Utility.BeforeSetMainAgent();
                        _controlTroopLogic.SetMainAgent();

                        // Set smooth move again if controls another agent instantly.
                        // Otherwise MissionScreen will reset camera elevate and bearing.
                        if (Mission.MainAgent != null && Mission.MainAgent.Controller == Agent.ControllerType.Player)
                            Utility.AfterSetMainAgent(shouldSmoothToAgent, _controlTroopLogic.MissionScreen);

                        // Restore the variables to initial state
                        else if (shouldSmoothToAgent)
                        {
                            Utility.ShouldSmoothMoveToAgent = true;
                            Utility.SetIsPlayerAgentAdded(_controlTroopLogic.MissionScreen, false);
                        }
                    }
                }
                else if (!Utility.IsTeamValid(Mission.PlayerTeam) || Mission.PlayerTeam.ActiveAgents.Count > 0)
                {
                    GameTexts.SetVariable("KeyName", RTSCameraGameKeyCategory.GetKey(GameKeyEnum.ControlTroop).ToSequenceString());
                    Utility.DisplayLocalizedText("str_rts_camera_control_troop_hint");
                }
            }

            if (Utility.IsTeamValid(affectedAgent.Team))
            {
                if (affectedAgent.Team.PlayerOrderController.Owner == affectedAgent)
                {
                    affectedAgent.Team.PlayerOrderController.Owner = null;
                }
            }
        }

        private void SwitchToAgent()
        {
            if (WatchBattleBehavior.WatchMode)
            {
                Utility.DisplayLocalizedText("str_rts_camera_cannot_control_agent_in_watch_mode");
                if (Mission.MainAgent == null)
                {
                    Utility.DisplayLocalizedText("str_rts_camera_player_dead");
                    _controlTroopLogic.SetMainAgent();
                }
                return;
            }
            IsSpectatorCamera = false;
            if (Mission.MainAgent != null)
            {
                Utility.DisplayLocalizedText("str_rts_camera_switch_to_player");
                _controlTroopLogic.ControlMainAgent();
            }
            else
            {
                Utility.DisplayLocalizedText("str_rts_camera_player_dead");
                _controlTroopLogic.SetMainAgent();
            }

            if (Mission.MainAgent != null)
            {
                Utilities.Utility.UpdateMainAgentControllerState(Mission.MainAgent, IsSpectatorCamera,
                    _config.GetPlayerControllerInFreeCamera());
            }

            MissionLibrary.Event.MissionEvent.OnToggleFreeCamera(false);
        }

        private void SwitchToFreeCamera()
        {
            IsSpectatorCamera = true;
            if (!Utility.IsPlayerDead())
            {
                UpdateMainAgentControllerInFreeCamera();
            }

            MissionLibrary.Event.MissionEvent.OnToggleFreeCamera(true);
            Utility.DisplayLocalizedText("str_rts_camera_switch_to_free_camera");
        }

        private void ToggleAutomaticSlowMotionMode(bool enable)
        {
			if (RTSCameraConfig.Get().SlowMotionModeOnRTSView)
			{
				var rtsCameraLogic = Mission.Current.GetMissionBehavior<RTSCameraLogic>();
				if (rtsCameraLogic != null)
				{
					rtsCameraLogic.MissionSpeedLogic.SetSlowMotionModeAutomatic(enable);
				}
			}
		}
    }
}