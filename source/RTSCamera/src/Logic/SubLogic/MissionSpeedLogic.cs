using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using TaleWorlds.MountAndBlade;

namespace RTSCamera.Logic.SubLogic
{
    public class MissionSpeedLogic
    {
        private readonly RTSCameraLogic _logic;
        private readonly RTSCameraConfig _config = RTSCameraConfig.Get();
        private const int TimeSpeedConstant = 23454;

        private bool IsAutomaticSpeedDilationActive = false;

		public Mission Mission => _logic.Mission;

        public MissionSpeedLogic(RTSCameraLogic logic)
        {
            _logic = logic;
        }

        public void AfterStart()
        {
        }

        public void OnMissionTick(float dt)
        {
            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.Pause).IsKeyPressed(Mission.InputManager))
            {
                TogglePause();
            }

            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SlowMotion).IsKeyPressed(Mission.InputManager))
            {
                SetSlowMotionMode(!_config.SlowMotionMode);
            }
        }

        public void TogglePause()
        {
            var paused = !MissionState.Current.Paused;
            MissionState.Current.Paused = paused;
            Utility.DisplayLocalizedText(paused ? "str_rts_camera_mission_paused" : "str_rts_camera_mission_continued");
        }

        public void SetSlowMotionMode(bool slowMotionMode)
        {
            if (_config.SlowMotionMode == slowMotionMode)
                return;

			_config.SlowMotionMode = slowMotionMode;
            Utility.DisplayLocalizedText(slowMotionMode ? "str_rts_camera_slow_motion_enabled" : "str_rts_camera_normal_mode_enabled");
            _config.Serialize();
        }

		public void SetSlowMotionModeAutomatic(bool slowMotionMode)
		{
			if (_config.SlowMotionMode == slowMotionMode)
				return;

			// automatic call && want normal speed && no automatic speedup previously
			if (!slowMotionMode && !IsAutomaticSpeedDilationActive)
				return;
			
            IsAutomaticSpeedDilationActive = slowMotionMode;
            SetSlowMotionMode(slowMotionMode);
		}


		public void SetSlowMotionFactor(float factor)
        {
            _config.SlowMotionFactor = factor;
        }
    }
}
