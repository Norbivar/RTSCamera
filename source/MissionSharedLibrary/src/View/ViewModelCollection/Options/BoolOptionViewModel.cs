using System;
using MissionLibrary.View;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace MissionSharedLibrary.View.ViewModelCollection.Options
{
    public class BoolOptionViewModel : OptionViewModel, IOption
    {
        private readonly Func<bool> _getValue;
        private readonly Action<bool> _setValue;

        [DataSourceProperty]
        public bool OptionValueAsBoolean
        {
            get => _getValue();
            set
            {
                if (value == _getValue())
                    return;
                if (_setValue != null)
                {
                    _setValue(value);
                    OnPropertyChanged(nameof(OptionValueAsBoolean));
                }
            }
        }

        public BoolOptionViewModel(TextObject name, TextObject description, Func<bool> getValue, Action<bool> setValue) 
            : base(name, description, OptionsVM.OptionsDataType.BooleanOption)
        {
            _getValue = getValue;
            _setValue = setValue;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            OnPropertyChanged(nameof(OptionValueAsBoolean));
        }

        public ViewModel GetViewModel()
        {
            return this;
        }

        public void Commit()
        { }

        public void Cancel()
        { }
    }
}
