using System;
using System.Collections.Generic;
using System.Linq;

namespace MissionLibrary.Provider
{
    public class ProviderManager : IProviderManager
    {
        private readonly Dictionary<Type, Dictionary<string, IVersionProvider>> _providersWithKey =
            new Dictionary<Type, Dictionary<string, IVersionProvider>>();

        public void RegisterProvider<T>(IVersionProvider<T> newProvider, string key = "") where T : ATag<T>
        {
            if (!_providersWithKey.TryGetValue(typeof(T), out var dictionary))
            {
                _providersWithKey.Add(typeof(T), new Dictionary<string, IVersionProvider>() {[key] = newProvider});
            }
            else if (!dictionary.TryGetValue(key, out var oldProvider))
            {
                dictionary.Add(key, newProvider);
            }
            else if (oldProvider.ProviderVersion.CompareTo(newProvider.ProviderVersion) <= 0)
            {
                dictionary[key] = newProvider;
            }
        }

        public T GetProvider<T>(string key = "") where T : ATag<T>
        {
            if (!_providersWithKey.TryGetValue(typeof(T), out var dictionary) || !dictionary.TryGetValue(key, out IVersionProvider provider) || !(provider is IVersionProvider<T> tProvider))
            {
                return null;
            }

            return tProvider.Value;
        }

        public IEnumerable<T> GetProviders<T>() where T : ATag<T>
        {
            if (!_providersWithKey.TryGetValue(typeof(T), out var dictionary))
            {
                return Enumerable.Empty<T>();
            }

            return dictionary.Values.Where(v => v is IVersionProvider<T>).Select(v => (v as IVersionProvider<T>)?.Value);
        }

        public void InstantiateAll()
        {
            foreach (var pair in _providersWithKey)
            {
                foreach (var versionProviderPair in pair.Value)
                {
                    versionProviderPair.Value.ForceCreate();
                }
            }
        }
    }
}
