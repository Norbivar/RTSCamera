using MissionLibrary.Provider;
using TaleWorlds.Library;

namespace MissionLibrary.View
{
    public interface IMenuClassCollection
    {
        public abstract void AddOptionClass(IIdProvider<AOptionClass> optionClass);

        public abstract void OnOptionClassSelected(AOptionClass optionClass);

        public abstract void Clear();

        public abstract ViewModel GetViewModel();
    }
}