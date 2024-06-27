using System.Collections.Generic;
using System.Linq;
using MissionLibrary.HotKey;
using TaleWorlds.InputSystem;

namespace MissionSharedLibrary.Config.HotKey
{
    public class SerializedGameKeySequence
    {
        public string StringId { get; set; }

        public List<InputKey> KeyboardKeys { get; set; }
    }

    public class SerializedGameKeyCategory
    {
        public string CategoryId { get; set; } = "DefaultGameKeyCategory";

        public List<SerializedGameKeySequence> GameKeySequences { get; set; } = new List<SerializedGameKeySequence>();

        public SerializedGameKeySequence GetGameKey(string gameKeyId)
        {
            for (int index = 0; index < this.GameKeySequences.Count; ++index)
            {
                SerializedGameKeySequence gameKeySequence = GameKeySequences[index];
                if (gameKeySequence != null && gameKeySequence.StringId == gameKeyId)
                    return gameKeySequence;
            }
            return null;
        }
    }
}
