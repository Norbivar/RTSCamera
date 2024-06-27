using System.Collections.Generic;

namespace MissionLibrary.Extension
{
    // Legacy
    public class MissionExtensionCollection
    {
        public static List<IMissionExtension> Extensions { get; } = new List<IMissionExtension>();

        public static void AddExtension(IMissionExtension extension)
        {
            Extensions.Add(extension);
        }

        public static void Clear()
        {
            Extensions.Clear();
        }
    }
}
