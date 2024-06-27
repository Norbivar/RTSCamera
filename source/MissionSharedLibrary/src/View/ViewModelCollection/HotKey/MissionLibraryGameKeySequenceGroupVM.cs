using MissionLibrary.HotKey;
using MissionSharedLibrary.Config.HotKey;
using System;
using System.Collections.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MissionSharedLibrary.View.ViewModelCollection.HotKey
{
    public class MissionLibraryGameKeySequenceGroupVM : AHotKeyConfigVM
    {
        private readonly Action<string, int, InputKey> _setAllKeysOfId;
        private readonly string _categoryId;
        private string _description;
        private MBBindingList<MissionLibraryGameKeySequenceOptionVM> _gameKeySequenceOptions;

        public MissionLibraryGameKeySequenceGroupVM(
            string categoryId,
            IEnumerable<GameKeySequence> keys,
            Action<MissionLibraryGameKeyOptionVM> onKeyBindRequest,
            Action<string, int, InputKey> setAllKeysOfId)
        {
            _setAllKeysOfId = setAllKeysOfId;
            _categoryId = categoryId;
            _gameKeySequenceOptions = new MBBindingList<MissionLibraryGameKeySequenceOptionVM>();
            foreach (GameKeySequence key in keys)
            {
                _gameKeySequenceOptions.Add(new MissionLibraryGameKeySequenceOptionVM(key, onKeyBindRequest, SetGameKey));
            }

            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Description = Module.CurrentModule.GlobalTextManager.FindText("str_key_category_name", _categoryId)
                .ToString();
            GameKeySequenceOptions.ApplyActionOnAllItems(x => x.RefreshValues());
        }

        private Key FindValidInputKey(GameKey gameKey)
        {
            return gameKey.KeyboardKey;
        }

        private void SetGameKey(MissionLibraryGameKeySequenceOptionVM option, InputKey newKey)
        {
            _setAllKeysOfId?.Invoke(_categoryId, option.GameKeySequence.Id, newKey);
        }

        public override void Update()
        {
            foreach (MissionLibraryGameKeySequenceOptionVM gameKey in this.GameKeySequenceOptions)
                gameKey.Update();
        }

        public override void OnReset()
        {
            foreach (MissionLibraryGameKeySequenceOptionVM option in GameKeySequenceOptions)
            {
                option.OnReset();
            }
        }

        public override void OnDone()
        {
            foreach (MissionLibraryGameKeySequenceOptionVM gameKey in GameKeySequenceOptions)
                gameKey.OnDone();
        }

        [DataSourceProperty]
        public MBBindingList<MissionLibraryGameKeySequenceOptionVM> GameKeySequenceOptions
        {
            get => _gameKeySequenceOptions;
            set
            {
                if (value == _gameKeySequenceOptions)
                    return;
                _gameKeySequenceOptions = value;
                OnPropertyChangedWithValue(value, nameof(GameKeySequenceOptions));
            }
        }

        [DataSourceProperty]
        public string Description
        {
            get => _description;
            set
            {
                if (value == _description)
                    return;
                _description = value;
                OnPropertyChangedWithValue(value, nameof(Description));
            }
        }
    }
}
