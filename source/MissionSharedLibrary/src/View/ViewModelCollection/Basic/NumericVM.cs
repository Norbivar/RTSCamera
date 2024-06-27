using System;
using TaleWorlds.Library;

namespace MissionSharedLibrary.View.ViewModelCollection.Basic
{
    public class NumericVM : ViewModel
    {

        private readonly float _initialValue;
        private float _min;
        private float _max;
        private float _optionValue;
        private bool _isDiscrete;
        private readonly Action<float> _updateAction;
        private readonly int _roundScale;
        private bool _isVisible;

        public NumericVM(string name, float initialValue, float min, float max, bool isDiscrete, Action<float> updateAction, int roundScale = 100, bool isVisible = true)
        {
            Name = name;
            _initialValue = initialValue;
            _min = min;
            _max = max;
            _optionValue = initialValue;
            _isDiscrete = isDiscrete;
            _updateAction = updateAction;
            _roundScale = roundScale;
            _isVisible = isVisible;
        }
        public string Name { get; }

        [DataSourceProperty]
        public float Min
        {
            get => _min;
            set
            {
                if (Math.Abs(value - _min) < 0.01f)
                    return;
                _min = value;
                OnPropertyChanged(nameof(Min));
            }
        }

        [DataSourceProperty]
        public float Max
        {
            get => _max;
            set
            {
                if (Math.Abs(value - _max) < 0.01f)
                    return;
                _max = value;
                OnPropertyChanged(nameof(Max));
            }
        }

        [DataSourceProperty]
        public float OptionValue
        {
            get => _optionValue;
            set
            {
                if (Math.Abs(value - (double)_optionValue) < 0.01f)
                    return;
                _optionValue = MathF.Round(value * _roundScale) / (float)_roundScale;
                OnPropertyChanged(nameof(OptionValue));
                OnPropertyChanged(nameof(OptionValueAsString));
                _updateAction(OptionValue);
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
                OnPropertyChanged(nameof(IsDiscrete));
            }
        }

        [DataSourceProperty]
        public string OptionValueAsString => !IsDiscrete ? _optionValue.ToString("F") : ((int)_optionValue).ToString();



        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value == _isVisible)
                    return;
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
    }
}
