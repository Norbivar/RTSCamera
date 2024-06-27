using TaleWorlds.Library;

namespace MissionLibrary.HotKey
{
    public abstract class AHotKeyConfigVM : ViewModel
    { 
        public abstract void Update();
        public abstract void OnReset();
        public abstract void OnDone();
    }
}
