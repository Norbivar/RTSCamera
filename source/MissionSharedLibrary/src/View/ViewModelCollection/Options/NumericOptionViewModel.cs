using System;
using MissionLibrary.View;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace MissionSharedLibrary.View.ViewModelCollection.Options
{
    public class NumericOptionViewModel : OptionViewModel, IOption
    {
        private readonly Func<float> _getValue;
        private readonly Action<float> _setValue;
        private float _min;
        private float _max;
        private bool _isDiscrete;
        private bool _updateContinuously;


        [DataSourceProperty]
        public float Min
        {
            get => _min;
            set
            {
                _min = value;
                OnPropertyChangedWithValue(value, nameof(Min));
            }
        }

        [DataSourceProperty]
        public float Max
        {
            get => _max;
            set
            {
                _max = value;
                OnPropertyChangedWithValue(value, nameof(Max));
            }
        }

        [DataSourceProperty]
        public float OptionValue
        {
            get =>  _getValue();
            set
            {
                _setValue(value);
                OnPropertyChangedWithValue(value, nameof(OptionValue));
                OnPropertyChanged(nameof(OptionValueAsString));
            }
        }

        [DataSourceProperty]
        public bool IsDiscrete
        {
            get => _isDiscrete;
            set
            {
                if (value == _isDiscrete)
                    return;
                _isDiscrete = value;
                OnPropertyChangedWithValue(value, nameof(IsDiscrete));
            }
        }

        [DataSourceProperty]
        public bool UpdateContinuously
        {
            get => _updateContinuously;
            set
            {
                if (value == _updateContinuously)
                    return;
                _updateContinuously = value;
                OnPropertyChangedWithValue(value, nameof(UpdateContinuously));
            }
        }

        [DataSourceProperty]
        public string OptionValueAsString => !IsDiscrete ? OptionValue.ToString("F") : ((int)OptionValue).ToString();

        public NumericOptionViewModel(TextObject name, TextObject description, Func<float> getValue,
            Action<float> setValue, float min, float max, bool isDiscrete, bool updateContinuously)
            : base(name, description, OptionsVM.OptionsDataType.NumericOption)
        {
            _getValue = getValue;
            _setValue = setValue;
            Min = min;
            Max = max;
            IsDiscrete = isDiscrete;
            UpdateContinuously = updateContinuously;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            OptionValue = _getValue();
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
