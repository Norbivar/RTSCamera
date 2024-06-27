namespace MissionLibrary.Provider
{
    public interface IProviderManager
    {
        void RegisterProvider<T>(IVersionProvider<T> newProvider, string key = "") where T : ATag<T>;
    }
}
