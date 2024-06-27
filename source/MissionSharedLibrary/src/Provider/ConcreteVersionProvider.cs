using System;
using MissionLibrary.Provider;

namespace MissionSharedLibrary.Provider
{
    public class ConcreteVersionProvider<T> : IVersionProvider<T> where T : ATag<T>
    {
        private readonly Func<ATag<T>> _creator;
        private T _value;
        public Version ProviderVersion { get; }

        public T Value => _value ??= Create();

        public ConcreteVersionProvider(Func<ATag<T>> creator, Version providerVersion)
        {
            ProviderVersion = providerVersion;
            _creator = creator;
        }
        public void ForceCreate()
        {
            _value ??= Create();
        }

        private T Create()
        {
            return _creator?.Invoke().Self;
        }
    }

    public class VersionProviderCreator
    {
        public static ConcreteVersionProvider<T> Create<T>(Func<ATag<T>> creator, Version providerVersion) where T : ATag<T>
        {
            return new ConcreteVersionProvider<T>(creator, providerVersion);
        }
    }
}
