using MissionLibrary.HotKey;
using MissionSharedLibrary.Config.HotKey;
using MissionSharedLibrary.Utilities;
using System;
using System.Linq;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MissionSharedLibrary.View.ViewModelCollection.HotKey
{
    public class MissionLibraryGameKeySequenceOptionVM : AHotKeyConfigVM
    {
        private readonly Action<MissionLibraryGameKeyOptionVM> _onKeybindRequest;
        private readonly Action<MissionLibraryGameKeySequenceOptionVM, InputKey> _onKeySet;
        private readonly string _groupId;
        private readonly string _id;
        private string _name;
        private string _description;
        private MBBindingList<MissionLibraryGameKeyOptionVM> _options;
        private bool _pushEnabled;
        private bool _popEnabled;

        public GameKeySequence GameKeySequence { get; private set; }

        public MissionLibraryGameKeySequenceOptionVM(
          GameKeySequence gameKeySequence,
          Action<MissionLibraryGameKeyOptionVM> onKeybindRequest,
          Action<MissionLibraryGameKeySequenceOptionVM, InputKey> onKeySet)
        {
            _onKeybindRequest = onKeybindRequest;
            _onKeySet = onKeySet;
            GameKeySequence = gameKeySequence;
            _groupId = GameKeySequence.CategoryId;
            _id = ((GameKeyDefinition)GameKeySequence.Id).ToString();
            UpdateOptions();
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Name = Module.CurrentModule.GlobalTextManager.FindText("str_key_name", _groupId + "_" + _id).ToString();
            Description = Module.CurrentModule.GlobalTextManager.FindText("str_key_description", _groupId + "_" + _id).ToString();
        }

        public override void Update()
        {
            foreach (var option in _options)
            {
                option.Update();
            }
        }

        public override void OnDone()
        {
            foreach (var option in _options)
            {
                option.OnDone();
            }

            GameKeySequence.SetGameKeys(Options.Select(vm => vm.Key.InputKey).ToList());
        }

        public override void OnReset()
        {
            GameKeySequence.ResetToDefault();
            
            UpdateOptions();
        }

        [DataSourceProperty]
        public bool PushEnabled
        {
            get => _pushEnabled;
            set
            {
                _pushEnabled = value;
                OnPropertyChanged(nameof(PushEnabled));
            }
        }

        public void PushGameKey()
        {
            Options.Add(new MissionLibraryGameKeyOptionVM(new Key(InputKey.Invalid), _onKeybindRequest, OnKeySet));
            UpdateButtons();
        }

        [DataSourceProperty]
        public bool PopEnabled
        {
            get => _popEnabled;
            set
            {
                _popEnabled = value;
                OnPropertyChanged(nameof(PopEnabled));
            }
        }

        public void PopGameKey()
        {
            Options.RemoveAt(Options.Count - 1);
            UpdateButtons();
        }

        public bool IsChanged()
        {
            return _options.Any(option => option.IsChanged());
        }

        private void OnKeySet(MissionLibraryGameKeyOptionVM option, InputKey key)
        {
            option.CurrentKey.ChangeKey(key);
            option.OptionValueText = Module.CurrentModule.GlobalTextManager
                .FindText("str_game_key_text", option.CurrentKey.ToString().ToLower()).ToString();
            _onKeySet?.Invoke(this, key);
        }

        private void UpdateButtons()
        {
            PopEnabled = Options.Count > (GameKeySequence.Mandatory ? 1 : 0);
            PushEnabled = Options.Count < 4;
        }

        private void UpdateOptions()
        {
            Options = new MBBindingList<MissionLibraryGameKeyOptionVM>();
            foreach (var key in GameKeySequence.Keys)
            {
                Options.Add(new MissionLibraryGameKeyOptionVM(key, _onKeybindRequest, OnKeySet));
            }
            UpdateButtons();
        }

        [DataSourceProperty]
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnPropertyChangedWithValue(value, nameof(Name));
            }
        }

        [DataSourceProperty]
        public string Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                OnPropertyChangedWithValue(value, nameof(Description));
            }
        }

        [DataSourceProperty]
        public MBBindingList<MissionLibraryGameKeyOptionVM> Options
        {
            get => _options;
            set
            {
                if (_options == value)
                    return;
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }
    }
}
