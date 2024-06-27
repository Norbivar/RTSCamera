namespace MissionLibrary.Provider
{
    public abstract class ATag<T> where T : ATag<T>
    {
        public virtual T Self => (T)this;
    }
}
