using MissionSharedLibrary.HotKey.Category;
using MissionSharedLibrary.Utilities;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

namespace MissionSharedLibrary.Controller.MissionBehaviors
{
    public class InputController : MissionLogic
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (GeneralGameKeyCategories.GetKey(GeneralGameKey.OpenMenu).IsKeyPressed(Mission.InputManager))
            {
                Utility.DisplayMessage("L pressed.");
            }
        }
    }
}
