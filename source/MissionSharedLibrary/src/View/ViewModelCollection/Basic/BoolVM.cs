using TaleWorlds.Library;

namespace MissionSharedLibrary.View.ViewModelCollection.Basic
{
    public class BoolVM : ViewModel
    {
        private bool _boolValue;

        [DataSourceProperty]
        public bool BoolValue
        {
            get => this._boolValue;
            set
            {
                if (value == this._boolValue)
                    return;
                this._boolValue = value;
                this.OnPropertyChangedWithValue((object)value, nameof(BoolValue));
            }
        }
    }
}
