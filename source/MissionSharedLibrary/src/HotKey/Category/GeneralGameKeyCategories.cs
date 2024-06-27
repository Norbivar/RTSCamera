using System;
using JetBrains.Annotations;
using MissionLibrary.Config.HotKey;
using MissionLibrary.HotKey;
using MissionSharedLibrary.Config.HotKey;
using System.Collections.Generic;
using TaleWorlds.InputSystem;

namespace MissionSharedLibrary.HotKey.Category
{
    public enum GeneralGameKey
    {
        OpenMenu,
        NumberOfGameKeyEnums
    }

    public class GeneralGameKeyCategories
    {
        public const string CategoryId = nameof(MissionLibrary) + nameof(GeneralGameKey);

        public static AGameKeyCategory GeneralGameKeyCategory => AGameKeyCategoryManager.Get()?.GetCategory(CategoryId);

        [NotNull]
        public static AGameKeyCategory CreateGeneralGameKeyCategory()
        {
            var result = new GameKeyCategory(CategoryId, (int) GeneralGameKey.NumberOfGameKeyEnums,
                GeneralGameKeyConfig.Get());

            result.AddGameKeySequence(new GameKeySequence((int) GeneralGameKey.OpenMenu,
                nameof(GeneralGameKey.OpenMenu),
                CategoryId, new List<InputKey>()
                {
                    InputKey.L
                }, true));
            return result;
        }

        public static void RegisterGameKeyCategory()
        {
            AGameKeyCategoryManager.Get()?.AddCategory(CreateGeneralGameKeyCategory, new Version(1, 0));
        }

        public static IGameKeySequence GetKey(GeneralGameKey key)
        {
            return GeneralGameKeyCategory.GetGameKeySequence((int) key);
        }
    }
}
