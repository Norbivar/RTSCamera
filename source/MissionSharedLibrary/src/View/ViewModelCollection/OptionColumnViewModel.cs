using System.Collections.Generic;
using MissionLibrary.View;
using TaleWorlds.Library;

namespace MissionSharedLibrary.View.ViewModelCollection
{
    public class OptionColumnViewModel : ViewModel
    {
        private readonly List<IOptionCategory> _optionCategories = new List<IOptionCategory>();
        private MBBindingList<ViewModel> _categories;

        [DataSourceProperty]
        public MBBindingList<ViewModel> Categories
        {
            get => _categories;
            set
            {
                if (_categories == value)
                    return;
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }

        public OptionColumnViewModel()
        {
            Categories = new MBBindingList<ViewModel>();
        }

        public void AddOptionCategory(IOptionCategory optionCategory)
        {
            var index = _optionCategories.FindIndex(o => o.Id == optionCategory.Id);
            if (index < 0)
            {
                _optionCategories.Add(optionCategory);
                Categories.Add(optionCategory.GetViewModel());
            }
            else
            {
                _optionCategories[index] = optionCategory;
                Categories[index] = optionCategory.GetViewModel();
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Refresh();
        }

        private void Refresh()
        {
            foreach (var viewModel in Categories)
            {
                viewModel.RefreshValues();
            }
        }
    }
}
