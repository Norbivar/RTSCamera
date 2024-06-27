using System;
using System.Collections.Generic;
using System.Linq;
using MissionLibrary.HotKey;
using MissionLibrary.View;
using MissionSharedLibrary.Utilities;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MissionSharedLibrary.View.ViewModelCollection.HotKey
{
    public class MissionLibraryGameKeyOptionCategoryVM : ViewModel
    {
        private readonly AGameKeyCategoryManager _gameKeyCategoryManager;
        private readonly Action<MissionLibraryGameKeyOptionVM> _onKeyBindRequest;
        private readonly Dictionary<GameKey, InputKey> _keysToChangeOnDone = new Dictionary<GameKey, InputKey>();
        private string _name;
        private string _resetText;
        private MBBindingList<AHotKeyConfigVM> _groups;
        private readonly Dictionary<string, AGameKeyCategory> _categories;

        public MissionLibraryGameKeyOptionCategoryVM(AGameKeyCategoryManager gameKeyCategoryManager, Action<IHotKeySetter> onKeyBindRequest)
        {
            _gameKeyCategoryManager = gameKeyCategoryManager;
            _onKeyBindRequest = onKeyBindRequest;
            _categories = _gameKeyCategoryManager.Categories.ToDictionary(pair => pair.Key, pair => pair.Value.Value);
            Groups = new MBBindingList<AHotKeyConfigVM>();
            foreach (KeyValuePair<string, AGameKeyCategory> category in _categories)
            {
                    Groups.Add(category.Value.CreateViewModel(onKeyBindRequest));
            }
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = new TextObject("{=Met1U45t}Mouse and Keyboard").ToString();
            Groups.ApplyActionOnAllItems(x => x.RefreshValues());
            ResetText = new TextObject("{=RVIKFCno}Reset to Defaults").ToString();
        }

        public void OnReset()
        {
            try
            {
                foreach (var group in this.Groups)
                    group.ExecuteCommand("OnReset", new object[]{});
                _keysToChangeOnDone.Clear();
            }
            catch (Exception e)
            {
                Utility.DisplayMessageForced(e.ToString());
            }
        }

        public void OnDone()
        {
            foreach (AHotKeyConfigVM group in Groups)
                group.OnDone();
            foreach (KeyValuePair<GameKey, InputKey> keyValuePair in _keysToChangeOnDone)
                FindValidInputKey(keyValuePair.Key).ChangeKey(keyValuePair.Value);
            _gameKeyCategoryManager.Save();
        }

        private Key FindValidInputKey(GameKey gameKey)
        {
            return gameKey.KeyboardKey;
        }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name)
                    return;
                _name = value;
                OnPropertyChangedWithValue(value, nameof(Name));
            }
        }

        [DataSourceProperty]
        public string ResetText
        {
            get => _resetText;
            set
            {
                if (_resetText == value)
                    return;
                _resetText = value;
                OnPropertyChangedWithValue(value, nameof(ResetText));
            }
        }

        [DataSourceProperty]
        public MBBindingList<AHotKeyConfigVM> Groups
        {
            get => _groups;
            set
            {
                if (value == _groups)
                    return;
                _groups = value;
                OnPropertyChangedWithValue(value, nameof(Groups));
            }
        }

        public void ExecuteResetToDefault()
        {
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=4gCU2ykB}Reset all keys to default").ToString(),
                new TextObject(
                        "{=YjbNtFcw}This will reset ALL keys to their default states. You won't be able to undo this action. {newline} {newline}Are you sure?")
                    .ToString(), true, true, new TextObject("{=aeouhelq}Yes").ToString(),
                new TextObject("{=8OkPHu4f}No").ToString(), OnReset, null));
        }
    }
}
