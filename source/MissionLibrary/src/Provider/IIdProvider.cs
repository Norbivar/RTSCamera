namespace MissionLibrary.Provider
{
    public interface IIdProvider
    {
        string Id { get; }
        void ForceCreate();
        void Clear();
    }

    public interface IIdProvider<out T> : IIdProvider where T : ATag<T>
    {
        T Value { get; }
    }
}
