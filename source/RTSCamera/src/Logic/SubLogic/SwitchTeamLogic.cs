﻿using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using RTSCamera.Event;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;

namespace RTSCamera.Logic.SubLogic
{
    public class SwitchTeamLogic
    {
        private readonly RTSCameraLogic _logic;
        private readonly RTSCameraConfig _config = RTSCameraConfig.Get();
        private ControlTroopLogic _controlTroopLogic;

        public Mission Mission => _logic.Mission;

        public SwitchTeamLogic(RTSCameraLogic logic)
        {
            _logic = logic;
        }

        public void OnBehaviourInitialize()
        {
            _controlTroopLogic = _logic.ControlTroopLogic;
        }

        public void OnMissionTick(float dt)
        {
            if (!NativeConfig.CheatMode)
                return;

            if (_config.SwitchTeamHotkeyEnabled && RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SwitchTeam).IsKeyPressed(Mission.InputManager))
                SwapTeam();
        }

        public void SwapTeam()
        {
            if (!NativeConfig.CheatMode)
                return;
            if (Mission.PlayerEnemyTeam == null)
                return;
            if (Mission.GetMissionBehaviour<SiegeDeploymentHandler>() != null)
                return;
            bool firstTime = Mission.PlayerEnemyTeam.PlayerOrderController.Owner == null;
            var targetAgent = Mission.PlayerEnemyTeam.PlayerOrderController.Owner;
            // Fix a rare crash in e1.4.3 when targetAgent.Team == null && targetAgent.IsDeleted == true and even **targetAgent.IsActive() == true**.
            targetAgent = !Utility.IsAgentDead(targetAgent) && targetAgent?.Team != null
                ? Mission.PlayerEnemyTeam.PlayerOrderController.Owner
                : !Utility.IsAgentDead(Mission.PlayerEnemyTeam.GeneralAgent) ? Mission.PlayerEnemyTeam.GeneralAgent : Mission.PlayerEnemyTeam.Leader;
            
            if (targetAgent == null)
            {
                Utility.DisplayLocalizedText("str_rts_camera_enemy_wiped_out");
                return;
            }
            Utility.DisplayLocalizedText("str_rts_camera_switch_to_enemy_team");

            MissionEvent.OnPreSwitchTeam();
            MissionLibrary.Event.MissionEvent.OnPreSwitchTeam();
            Mission.PlayerEnemyTeam.PlayerOrderController.Owner = targetAgent;
            Mission.PlayerTeam = Mission.PlayerEnemyTeam;
            _controlTroopLogic.SetToMainAgent(targetAgent);
            MissionLibrary.Event.MissionEvent.OnPostSwitchTeam();
            MissionEvent.OnPostSwitchTeam();

            if (firstTime)
            {
                foreach (var formation in Mission.PlayerTeam.FormationsIncludingEmpty)
                {
                    bool isAIControlled = formation.IsAIControlled;
                    formation.PlayerOwner = Mission.MainAgent;
                    formation.IsAIControlled = isAIControlled;
                }
            }
        }
    }
}
