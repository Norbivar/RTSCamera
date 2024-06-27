using System;
using MissionLibrary.Provider;

namespace MissionSharedLibrary.Provider
{
    public class ConcreteIdProvider<T> : IIdProvider<T> where T : ATag<T>
    {
        private readonly Func<ATag<T>> _creator;
        private T _value;
        public string Id { get; }

        public T Value => _value ??= Create();

        public ConcreteIdProvider(Func<ATag<T>> creator, string id)
        {
            Id = id;
            _creator = creator;
        }
        public void ForceCreate()
        {
            _value ??= Create();
        }

        public void Clear()
        {
            _value = null;
        }

        private T Create()
        {
            return _creator?.Invoke().Self;
        }
    }

    public class IdProviderCreator
    {
        public static ConcreteIdProvider<T> Create<T>(Func<ATag<T>> creator, string id) where T : ATag<T>
        {
            return new ConcreteIdProvider<T>(creator, id);
        }
    }
}
