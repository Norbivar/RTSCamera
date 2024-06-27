using MissionSharedLibrary.View.ViewModelCollection.Basic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace MissionSharedLibrary.View.ViewModelCollection.Options
{
    public abstract class OptionViewModel : ViewModel
    {
        private readonly TextObject _descriptionText;
        private int _optionTypeId = -1;
        private string[] _imageIDs;
        private HintViewModel _description;
        private bool _isEnabled;

        public TextViewModel Name { get; }

        [DataSourceProperty]
        public HintViewModel Description
        {
            get => _description;
            set
            {
                if (_description == value)
                    return;
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [DataSourceProperty]
        public string[] ImageIDs
        {
            get => _imageIDs;
            set
            {
                if (value == _imageIDs)
                    return;
                _imageIDs = value;
                OnPropertyChangedWithValue(value, nameof(ImageIDs));
            }
        }

        [DataSourceProperty]
        public int OptionTypeID
        {
            get => _optionTypeId;
            set
            {
                if (value == _optionTypeId)
                    return;
                _optionTypeId = value;
                OnPropertyChangedWithValue(value, nameof(OptionTypeID));
            }
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled)
                    return;
                _isEnabled = value;
                OnPropertyChangedWithValue(value, nameof(IsEnabled));
            }
        }

        protected OptionViewModel(
            TextObject name,
            TextObject description,
            OptionsVM.OptionsDataType typeID,
            bool isEnabled = true)
        {
            _descriptionText = description;

            Name = new TextViewModel(name);
            if (_descriptionText != null)
                Description = new HintViewModel(_descriptionText);
            OptionTypeID = (int)typeID;
            IsEnabled = isEnabled;

            Refresh();
        }

        public virtual void UpdateData(bool initUpdate)
        {
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Refresh();
        }

        private void Refresh()
        {
            Name.RefreshValues();
            if (_descriptionText != null)
                Description = new HintViewModel(_descriptionText);
        }
    }
}
