using MissionSharedLibrary.Utilities;
using RTSCamera.CampaignGame.Behavior;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using RTSCamera.View;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;

namespace RTSCamera.Logic.SubLogic
{
    public class ControlTroopLogic
    {
        private RTSCameraLogic _rtsCameraLogic;
        private readonly RTSCameraConfig _config = RTSCameraConfig.Get();
        private SwitchFreeCameraLogic _switchFreeCameraLogic;
        private FlyCameraMissionView _flyCameraMissionView;
        private RTSCameraSelectCharacterView _selectCharacterView;

        public Mission Mission => _rtsCameraLogic.Mission;

        public MissionScreen MissionScreen => _flyCameraMissionView?.MissionScreen;

        public ControlTroopLogic(RTSCameraLogic logic)
        {
            _rtsCameraLogic = logic;
        }

        public bool SetMainAgent()
        {
            return SetToMainAgent(GetAgentToControl());
        }

        public bool SetToMainAgent(Agent agent)
        {
            if (agent != null)
            {
                if (Mission.MainAgent == agent || agent.Team != Mission.PlayerTeam)
                    return false;

                if (!Utility.IsPlayerDead())
                {
                    MissionLibrary.Event.MissionEvent.OnMainAgentWillBeChangedToAnotherOne(agent);
                    // Let AI control previous main agent.
                    Utility.AIControlMainAgent(false);
                }

                GameTexts.SetVariable("ControlledTroopName", agent.Name);
                Utility.DisplayLocalizedText("str_rts_camera_control_troop");
                bool shouldSmoothMoveToAgent = Utility.BeforeSetMainAgent();
                if (_switchFreeCameraLogic.IsSpectatorCamera || WatchBattleBehavior.WatchMode)
                {
                    Mission.MainAgent = agent;
                    Utility.AfterSetMainAgent(shouldSmoothMoveToAgent, _flyCameraMissionView.MissionScreen, false);
                }
                else
                {
                    Utility.PlayerControlAgent(agent);
                    Utility.AfterSetMainAgent(shouldSmoothMoveToAgent, _flyCameraMissionView.MissionScreen);
                }

                return true;
            }

            if (Utilities.Utility.IsBattleCombat(Mission))
            {
                Utility.DisplayLocalizedText("str_rts_camera_no_troop_to_control");
            }
            return false;
        }

        public bool ForceControlAgent()
        {
            return ForceControlAgent(GetAgentToControl());
        }

        public bool ForceControlAgent(Agent agent)
        {
            try
            {
                if (agent != null)
                {
                    if ((!_switchFreeCameraLogic.IsSpectatorCamera && agent.Controller == Agent.ControllerType.Player) || agent.Team != Mission.PlayerTeam)
                        return false;

                    if (!Utility.IsPlayerDead() && Mission.MainAgent != agent)
                    {
                        MissionLibrary.Event.MissionEvent.OnMainAgentWillBeChangedToAnotherOne(agent);
                        // Let AI control previous main agent.
                        Utility.AIControlMainAgent(false);
                    }
                    bool shouldSmoothMoveToAgent = Utility.BeforeSetMainAgent();
                    if (_switchFreeCameraLogic.IsSpectatorCamera)
                    {
                        agent.GetTargetAgent();
                        if (Mission.MainAgent != agent)
                            Mission.MainAgent = agent;

                        _switchFreeCameraLogic.SwitchCamera();
                    }
                    else
                    {
                        Utility.PlayerControlAgent(agent);
                        _flyCameraMissionView.DisableControlHint();
                    }

                    Utility.AfterSetMainAgent(shouldSmoothMoveToAgent, _flyCameraMissionView.MissionScreen);

                    return true;
                }

                Utility.DisplayLocalizedText("str_rts_camera_no_troop_to_control");
                return false;
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
            }

            return false;
        }

        public bool ControlMainAgent(bool displayMessage = true)
        {
            try
            {
                if (Mission.MainAgent != null)
                {
                    if (displayMessage)
                    {
                        GameTexts.SetVariable("ControlledTroopName", Mission.MainAgent.Name);
                        Utility.DisplayLocalizedText("str_rts_camera_control_troop");
                    }

                    bool shouldSmoothMoveToAgent = Utility.BeforeSetMainAgent();
                    Utility.PlayerControlAgent(Mission.MainAgent);
                    Utility.AfterSetMainAgent(shouldSmoothMoveToAgent, _flyCameraMissionView.MissionScreen);

                    return true;
                }
                else
                {
                    Utility.DisplayLocalizedText("str_rts_camera_no_troop_to_control");
                }
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
            }

            return false;
        }

        public Agent GetAgentToControl()
        {
            if (_flyCameraMissionView.MissionScreen?.LastFollowedAgent?.IsActive() ?? false)
            {
                if ((!_switchFreeCameraLogic.IsSpectatorCamera || _flyCameraMissionView.LockToAgent) &&
                    _flyCameraMissionView.MissionScreen.LastFollowedAgent.Team == Mission.PlayerTeam) return _flyCameraMissionView.MissionScreen?.LastFollowedAgent;
            }
            else if (Mission.MainAgent?.IsActive() ?? false)
            {
                return Mission.MainAgent;
            }

            if (!Utility.IsTeamValid(Mission.PlayerTeam))
                return null;

            return GetOtherAgentToControl(true) ??
                   (RTSCameraConfig.Get().IgnoreRetreatingTroops && !_switchFreeCameraLogic.IsSpectatorCamera ? null : GetOtherAgentToControl(false));
        }

        private Agent GetOtherAgentToControl(bool ignoreRetreatingAgents)
        {
            var cameraPosition = Mission.Scene.LastFinalRenderCameraPosition;
            if (_config.PreferUnitsInSameFormation)
            {
                var firstPreference = AgentPreferenceFromFormation(_switchFreeCameraLogic.CurrentPlayerFormation,
                    cameraPosition, ignoreRetreatingAgents);
                if (firstPreference.agent != null)
                {
                    return firstPreference.agent;
                }

                if (_switchFreeCameraLogic.CurrentPlayerFormation != _config.PlayerFormation)
                {
                    var secondPreference = AgentPreferenceFromFormation((FormationClass)_config.PlayerFormation,
                        cameraPosition, ignoreRetreatingAgents);
                    if ((secondPreference.hero ?? secondPreference.agent) != null)
                    {
                        return secondPreference.hero ?? secondPreference.agent;
                    }
                }

                var thirdPreference = AgentPreferenceFromPlayerTeam(cameraPosition, ignoreRetreatingAgents);
                return thirdPreference.hero ?? thirdPreference.agent;
            }
            else
            {
                var firstPreference =
                    AgentPreferenceFromFormation(_switchFreeCameraLogic.CurrentPlayerFormation, cameraPosition,
                        ignoreRetreatingAgents);
                if (firstPreference.hero != null)
                    return firstPreference.hero;

                if (_switchFreeCameraLogic.CurrentPlayerFormation != _config.PlayerFormation)
                {
                    var secondPreference =
                        AgentPreferenceFromFormation((FormationClass)_config.PlayerFormation, cameraPosition,
                            ignoreRetreatingAgents);
                    if (secondPreference.hero != null)
                        return secondPreference.hero;
                    var thirdPreference = AgentPreferenceFromPlayerTeam(cameraPosition, ignoreRetreatingAgents);
                    return thirdPreference.hero ??
                           firstPreference.agent ?? secondPreference.agent ?? thirdPreference.agent;
                }
                else
                {
                    var thirdPreference = AgentPreferenceFromPlayerTeam(cameraPosition, ignoreRetreatingAgents);
                    return thirdPreference.hero ??
                           firstPreference.agent ?? thirdPreference.agent;
                }
            }
        }

        private (Agent agent, Agent hero) AgentPreferenceFromPlayerTeam(Vec3 position, bool ignoreRetreatingAgents)
        {
            var preference = new ControlAgentPreference();
            preference.UpdateAgentPreferenceFromTeam(Mission.PlayerTeam, position, ignoreRetreatingAgents);
            return (preference.BestAgent, preference.BestHero);
        }


        private (Agent agent, Agent hero) AgentPreferenceFromFormation(FormationClass formationClass,
            Vec3 position, bool ignoreRetreatingAgents)
        {
            var preference = new ControlAgentPreference();
            preference.UpdateAgentPreferenceFromFormation(formationClass, position, ignoreRetreatingAgents);
            return (preference.BestAgent, preference.BestHero);
        }

        public void OnBehaviourInitialize()
        {
            _rtsCameraLogic = Mission.GetMissionBehavior<RTSCameraLogic>();
            _switchFreeCameraLogic = _rtsCameraLogic.SwitchFreeCameraLogic;
            _flyCameraMissionView = Mission.GetMissionBehavior<FlyCameraMissionView>();
            _selectCharacterView = Mission.GetMissionBehavior<RTSCameraSelectCharacterView>();
        }

        public void OnMissionTick(float dt)
        {
            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.ControlTroop).IsKeyPressed(Mission.InputManager))
            {
                if (_selectCharacterView.LockOnAgent(GetAgentToControl()))
                    return;

                if (!_switchFreeCameraLogic.IsSpectatorCamera && Mission.MainAgent?.Controller == Agent.ControllerType.Player)
                    return;

                ForceControlAgent();
            }
        }
    }
}