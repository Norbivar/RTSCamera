using TaleWorlds.Library;

namespace MissionLibrary.View
{
    public interface IOption : IViewModelProvider<ViewModel>
    {
        void Commit();
        void Cancel();
    }
}
