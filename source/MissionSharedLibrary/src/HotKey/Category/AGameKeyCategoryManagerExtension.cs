using System;
using MissionLibrary.HotKey;
using MissionSharedLibrary.Provider;

namespace MissionSharedLibrary.HotKey.Category
{
    public static class AGameKeyCategoryManagerExtension
    {
        public static void AddCategory(this AGameKeyCategoryManager categoryManager, Func<AGameKeyCategory> creator,
            Version version, bool addOnlyWhenMissing = true)
        {
            categoryManager.AddCategory(new ConcreteVersionProvider<AGameKeyCategory>(creator, version), addOnlyWhenMissing);
        }
    }
}
