using System.Collections.Generic;
using MissionLibrary.Provider;
using System;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace MissionLibrary.HotKey
{
    public abstract class AGameKeyCategory : ATag<AGameKeyCategory>
    {
        public abstract string GameKeyCategoryId { get; }

        public abstract IGameKeySequence GetGameKeySequence(int i);

        public abstract void Save();

        public abstract void Load();
        public abstract AHotKeyConfigVM CreateViewModel(Action<IHotKeySetter> onKeyBindRequest);
    }
}
