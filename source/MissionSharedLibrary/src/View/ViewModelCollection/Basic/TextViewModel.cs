using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MissionSharedLibrary.View.ViewModelCollection.Basic
{
    public class TextViewModel : ViewModel
    {
        private TextObject _textObject;
        private string _text;
        private bool _isVisible;

        public TextObject TextObject
        {
            get => _textObject;
            set
            {
                _textObject = value;
                Text = _textObject.ToString();
            }
        }

        [DataSourceProperty]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                    return;
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public TextViewModel(TextObject text, bool isVisible = true)
        {
            TextObject = text;
            IsVisible = isVisible;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            TextObject = TextObject;
        }
    }
}
