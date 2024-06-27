using MissionLibrary.Provider;
using System.Collections.Generic;

namespace MissionLibrary
{
    public static class Global
    {
        private static bool IsInitialized { get; set; }
        private static bool IsSecondInitialized { get; set; }

        private static ProviderManager ProviderManager { get; set; }

        public static void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            ProviderManager = new ProviderManager();
        }

        public static void SecondInitialize()
        {
            if (IsSecondInitialized)
                return;

            IsSecondInitialized = true;
            ProviderManager.InstantiateAll();
        }

        public static void RegisterProvider<T>(IVersionProvider<T> newProvider, string key = "") where T : ATag<T>
        {
            ProviderManager.RegisterProvider(newProvider, key);
        }

        public static T GetProvider<T>(string key = "") where T : ATag<T>
        {
            return ProviderManager.GetProvider<T>(key);
        }

        public static IEnumerable<T> GetProviders<T>() where T : ATag<T>
        {
            return ProviderManager.GetProviders<T>();
        }

        public static void Clear()
        {
            if (!IsInitialized)
                return;

            IsInitialized = false;
            IsSecondInitialized = false;
            ProviderManager = null;
        }
    }
}
