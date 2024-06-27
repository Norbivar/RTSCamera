using System.Collections.Generic;
using MissionLibrary.View;
using MissionSharedLibrary.View.ViewModelCollection.Basic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace MissionSharedLibrary.View.ViewModelCollection
{
    public class OptionCategory : ViewModel, IOptionCategory
    {
        private readonly List<IOption> _options = new List<IOption>();
        private MBBindingList<ViewModel> _optionViewModels;

        public string Id { get; }

        public TextViewModel Title { get; }

        [DataSourceProperty]
        public MBBindingList<ViewModel> OptionViewModels
        {
            get => _optionViewModels;
            set
            {
                if (_optionViewModels == value)
                    return;
                _optionViewModels = value;
                OnPropertyChanged(nameof(OptionViewModels));
            }
        }

        public OptionCategory(string id, TextObject title)
        {
            Id = id;
            Title = new TextViewModel(title);
            OptionViewModels = new MBBindingList<ViewModel>();
        }

        public void AddOption(IOption option)
        {
            _options.Add(option);
            OptionViewModels.Add(option.GetViewModel());
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            foreach (var optionViewModel in OptionViewModels)
            {
                optionViewModel.RefreshValues();
            }
        }

        public ViewModel GetViewModel()
        {
            return this;
        }
    }
}
