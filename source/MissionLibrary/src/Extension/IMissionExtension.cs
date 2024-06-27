using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace MissionLibrary.Extension
{
    // Legacy
    public interface IMissionExtension
    {
        void OpenExtensionMenu(Mission mission);

        string ExtensionName { get; }
        string ButtonName { get; }

        List<MissionBehavior> CreateMissionBehaviors(Mission mission);
    }
}
