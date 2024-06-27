using System;
using MissionLibrary.View;
using MissionSharedLibrary.View.ViewModelCollection.Basic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MissionSharedLibrary.View.ViewModelCollection
{
    public class OptionClass : AOptionClass
    {
        private readonly OptionClassViewModel _viewModel;
        private readonly IMenuClassCollection _menuClassCollection;

        public OptionClass(string id, TextObject name, IMenuClassCollection menuClassCollection)
        {
            Id = id;
            _viewModel = new OptionClassViewModel(id, name, OnSelect);
            _menuClassCollection = menuClassCollection;
        }

        public void AddOptionCategory(int column, IOptionCategory optionCategory)
        {
            _viewModel.AddOptionCategory(column, optionCategory);
        }

        public override string Id { get; }

        public override ViewModel GetViewModel()
        {
            return _viewModel;
        }

        public override void UpdateSelection(bool isSelected)
        {
            _viewModel.IsSelected = isSelected;
        }

        private void OnSelect()
        {
            _menuClassCollection.OnOptionClassSelected(this);
        }
    }

    public class OptionClassViewModel : ViewModel
    {
        private readonly int _maxColumnIndex = 10;
        private MBBindingList<OptionColumnViewModel> _optionColumns = new MBBindingList<OptionColumnViewModel>();
        private readonly Action _onSelect;

        public string Id { get; }
        public TextViewModel Name { get; }

        [DataSourceProperty]
        public MBBindingList<OptionColumnViewModel> OptionColumns
        {
            get => _optionColumns;
            set
            {
                if (_optionColumns == value)
                    return;
                _optionColumns = value;
                OnPropertyChanged(nameof(OptionColumns));
            }
        }

        private bool _isSelected;

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (value == this._isSelected)
                    return;
                this._isSelected = value;
                this.OnPropertyChangedWithValue((object)value, nameof(IsSelected));
            }
        }

        public OptionClassViewModel(string id, TextObject name, Action onSelect)
        {
            Id = id;
            Name = new TextViewModel(name);
            _onSelect = onSelect;
        }

        public void AddOptionCategory(int column, IOptionCategory optionCategory)
        {
            column = Math.Min(column, _maxColumnIndex);
            if (column < 0 || column >= OptionColumns.Count)
            {
                column = MBMath.ClampInt(column, 0, OptionColumns.Count);
                OptionColumns.Insert(column, new OptionColumnViewModel());
            }

            OptionColumns[column].AddOptionCategory(optionCategory);
        }

        public void ExecuteSelection()
        {
            _onSelect();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Refresh();
        }

        private void Refresh()
        {
            foreach (var optionColumnViewModel in _optionColumns)
            {
                optionColumnViewModel.RefreshValues();
            }
        }
    }
}
