using System;

namespace MissionLibrary.Provider
{
    public interface IVersionProvider
    {
        Version ProviderVersion { get; }
        void ForceCreate();
    }

    public interface IVersionProvider<out T>: IVersionProvider where T : ATag<T>
    {
        T Value { get; }
    }
}
