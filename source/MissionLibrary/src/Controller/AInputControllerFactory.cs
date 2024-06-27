using MissionLibrary.Provider;
using TaleWorlds.MountAndBlade;

namespace MissionLibrary.Controller
{
    public abstract class AInputControllerFactory : ATag<AInputControllerFactory>
    {
        public abstract MissionLogic CreateInputController(Mission mission);
    }
}
