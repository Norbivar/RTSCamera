using System;
using TaleWorlds.Library;

namespace MissionLibrary.View
{
    public interface IViewModelProvider<out T> where T : ViewModel
    {
        T GetViewModel();
    }

    public interface IViewModelProvider<out T, out U, in V> where T : ViewModel
    {
        T GetViewModel(Func<U, V> func);
    }
}
