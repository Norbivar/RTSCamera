using MissionLibrary.Controller.Camera;

namespace MissionSharedLibrary.Controller.Camera
{
    public class CameraControllerManager : ACameraControllerManager
    {

        public override ICameraController Instance { get; set; }

        public override void Clear()
        {
            Instance = null;
        }
    }
}
