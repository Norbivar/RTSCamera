using MissionSharedLibrary.Config;
using MissionSharedLibrary.Config.HotKey;
using System.IO;

namespace MissionLibrary.Config.HotKey
{
    public class GeneralGameKeyConfig : GameKeyConfigBase<GeneralGameKeyConfig>
    {
        protected override string SaveName { get; } = Path.Combine(ConfigPath.ConfigDir, nameof(MissionLibrary), nameof(GeneralGameKeyConfig) + ".xml");
    }
}
