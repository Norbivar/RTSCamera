using MissionLibrary.View;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace MissionSharedLibrary.View.ViewModelCollection.Options.Selection
{
    public class SelectionOptionViewModel : OptionViewModel, IOption
    {
        private readonly SelectionOptionData _selectionOptionData;
        private readonly bool _commitOnlyWhenChange;
        private SelectorVM<SelectorItemVM> _selector;
        private bool _includeOrdinal;

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> Selector
        {
            get => _selector;
            set
            {
                if (value == _selector)
                    return;
                _selector = value;
                OnPropertyChangedWithValue(value, nameof(Selector));
            }
        }

        public SelectionOptionViewModel(TextObject name, TextObject description, SelectionOptionData selectionOptionData, bool commitOnlyWhenChange, bool includeOrdinal = false) 
            : base(name, description, OptionsVM.OptionsDataType.MultipleSelectionOption)
        {
            _selectionOptionData = selectionOptionData;
            _commitOnlyWhenChange = commitOnlyWhenChange;
            _includeOrdinal = includeOrdinal;
            Selector = new SelectorVM<SelectorItemVM>(0, null);
            UpdateData(true);
            Selector.SelectedIndex = _selectionOptionData.GetDefaultValue();
        }

        public ViewModel GetViewModel()
        {
            return this;
        }

        public void Commit()
        {
            _selectionOptionData.Commit();
        }

        public void Cancel()
        {
            Selector.SelectedIndex = _selectionOptionData.GetDefaultValue();
            UpdateValue(_selector);
        }

        public override void UpdateData(bool initialUpdate)
        {
            base.UpdateData(initialUpdate);
            IEnumerable<SelectionItem> selectableOptionNames = _selectionOptionData.GetSelectableOptionNames();
            Selector.SetOnChangeAction(null);
            Selector.SelectedIndex = -1;
            var selectionItems = selectableOptionNames as SelectionItem[] ?? selectableOptionNames.ToArray();
            if (selectionItems.Any() && selectionItems.All(n => n.IsLocalizationId))
            {
                List<TextObject> textObjectList = new List<TextObject>();
                foreach (var (selectionData, i) in selectionItems.Select((item, i) => (item, i)))
                {
                    TextObject text = GameTexts.FindText(selectionData.Data, selectionData.Variation);
                    textObjectList.Add(_includeOrdinal
                        ? new TextObject($"{i + 1}: " + "{Text}").SetTextVariable("Text", text)
                        : text);
                }
                Selector.Refresh(textObjectList, _selectionOptionData.GetValue(), UpdateValue);
            }
            else
            {
                List<string> stringList = new List<string>();
                foreach (SelectionItem selectionData in selectionItems)
                {
                    if (selectionData.IsLocalizationId)
                    {
                        TextObject text = GameTexts.FindText(selectionData.Data);
                        stringList.Add(text.ToString());
                    }
                    else
                        stringList.Add(selectionData.Data);
                }
                Selector.Refresh(stringList, _selectionOptionData.GetValue(), UpdateValue);
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Selector?.RefreshValues();
        }

        private void UpdateValue(SelectorVM<SelectorItemVM> selector)
        {
            if (selector.SelectedIndex < 0)
                return;
            _selectionOptionData.SetValue(selector.SelectedIndex);
            if (!_commitOnlyWhenChange || _selectionOptionData.GetValue() != _selectionOptionData.GetDefaultValue())
                _selectionOptionData.Commit();
        }
    }
}
