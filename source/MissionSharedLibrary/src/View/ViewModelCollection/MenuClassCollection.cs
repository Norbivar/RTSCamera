using MissionLibrary.Provider;
using MissionLibrary.View;
using MissionSharedLibrary.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace MissionSharedLibrary.View
{
    public class MenuClassCollection : IMenuClassCollection
    {
        private readonly List<IIdProvider<AOptionClass>> _optionClasses = new List<IIdProvider<AOptionClass>>();
        private MenuClassCollectionViewModel _viewModel;
        private string _previouslySelectedOptionClassId = "";

        public void AddOptionClass(IIdProvider<AOptionClass> provider)
        {
            var index = _optionClasses.FindIndex(o => o.Id == provider.Id);
            if (index < 0)
                _optionClasses.Add(provider);
            else
                _optionClasses[index] = provider;
        }

        public void OnOptionClassSelected(AOptionClass optionClass)
        {
            _previouslySelectedOptionClassId = optionClass.Id;
            _viewModel?.OnOptionClassSelected(optionClass);
        }

        public void Clear()
        {
            _viewModel = null;
            foreach (var optionClass in _optionClasses)
            {
                optionClass.Clear();
            }
        }

        public ViewModel GetViewModel()
        {
            return _viewModel ??= new MenuClassCollectionViewModel(_optionClasses, _previouslySelectedOptionClassId);
        }
    }

    public class MenuClassCollectionViewModel : ViewModel
    {
        private MBBindingList<ViewModel> _optionClassViewModels;
        private AOptionClass _currentSelectedOptionClass;
        private ViewModel _currentOptionClassViewModel;

        public AOptionClass CurrentSelectedOptionClass
        {
            get => _currentSelectedOptionClass;
            private set
            {
                if (_currentSelectedOptionClass == value)
                    return;
                _currentSelectedOptionClass = value;
                CurrentOptionClassViewModel = _currentSelectedOptionClass?.GetViewModel();
            }
        }

        [DataSourceProperty]
        public MBBindingList<ViewModel> OptionClassViewModels
        {
            get => _optionClassViewModels;
            set
            {
                if (_optionClassViewModels == value)
                    return;
                _optionClassViewModels = value;
                OnPropertyChanged(nameof(OptionClassViewModels));
            }
        }

        [DataSourceProperty]
        public ViewModel CurrentOptionClassViewModel
        {
            get => _currentOptionClassViewModel;
            set
            {
                if (_currentOptionClassViewModel == value)
                    return;
                _currentOptionClassViewModel = value;
                OnPropertyChanged(nameof(CurrentOptionClassViewModel));
            }
        }

        public void OnOptionClassSelected(AOptionClass optionClass)
        {
            if (CurrentSelectedOptionClass == optionClass)
                return;
            CurrentSelectedOptionClass?.UpdateSelection(false);
            CurrentSelectedOptionClass = optionClass;
            CurrentSelectedOptionClass?.UpdateSelection(true);
        }

        public MenuClassCollectionViewModel(List<IIdProvider<AOptionClass>> optionClasses, string selectedOptionClassId)
        {
            var optionClassViewModels = new MBBindingList<ViewModel>();
            foreach (var optionClass in optionClasses)
            {
                try
                {
                    optionClassViewModels.Add(optionClass.Value.GetViewModel());
                }
                catch (Exception e)
                {
                    Utility.DisplayMessageForced(e.ToString());
                    Console.WriteLine(e);
                }
            }
            OptionClassViewModels = optionClassViewModels;
            try
            {
                OnOptionClassSelected(
                    (optionClasses.FirstOrDefault(optionClass => optionClass.Value.Id == selectedOptionClassId) ??
                     optionClasses.FirstOrDefault())?.Value);
            }
            catch (Exception e)
            {
                Utility.DisplayMessageForced(e.ToString());
                Console.WriteLine(e);
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            Refresh();
        }

        private void Refresh()
        {
            foreach (var optionClassViewModel in OptionClassViewModels)
            {
                optionClassViewModel.RefreshValues();
            }
        }
    }
}
