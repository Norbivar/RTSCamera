using System.Collections.Generic;
using MissionLibrary.HotKey;
using MissionLibrary.Provider;

namespace MissionSharedLibrary.HotKey
{
    public class GameKeyCategoryManager : AGameKeyCategoryManager
    {

        public override Dictionary<string, IVersionProvider<AGameKeyCategory>> Categories { get; } = new Dictionary<string, IVersionProvider<AGameKeyCategory>>();

        public override void AddCategory(IVersionProvider<AGameKeyCategory> provider, bool addOnlyWhenMissing = true)
        {
            if (Categories.TryGetValue(provider.Value.GameKeyCategoryId, out IVersionProvider<AGameKeyCategory> existingProvider))
            {
                if (existingProvider.ProviderVersion == provider.ProviderVersion && addOnlyWhenMissing ||
                    existingProvider.ProviderVersion > provider.ProviderVersion)
                    return;

                Categories[provider.Value.GameKeyCategoryId] = provider;
            }

            Categories.Add(provider.Value.GameKeyCategoryId, provider);

            provider.Value.Load();
            provider.Value.Save();
        }

        public override AGameKeyCategory GetCategory(string categoryId)
        {
            if (Categories.TryGetValue(categoryId, out IVersionProvider<AGameKeyCategory> provider))
            {
                return provider.Value;
            }

            return null;
        }

        public override T GetCategory<T>(string categoryId)
        {
            if (Categories.TryGetValue(categoryId, out IVersionProvider<AGameKeyCategory> provider) && provider.Value is T t)
            {
                return t;
            }

            return null;
        }

        public override void Save()
        {
            foreach(var pair in Categories)
            {
                pair.Value.Value.Save();
            }
        }
    }
}
