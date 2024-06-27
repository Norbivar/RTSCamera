using TaleWorlds.Library;

namespace MissionLibrary.View
{
    public interface IOptionCategory : IViewModelProvider<ViewModel>
    {
        string Id { get; }
    }
}
