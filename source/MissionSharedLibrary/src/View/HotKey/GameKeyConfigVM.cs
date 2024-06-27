using System;
using MissionLibrary.HotKey;
using MissionSharedLibrary.View.ViewModelCollection.HotKey;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ViewModelCollection.GameOptions;

namespace MissionSharedLibrary.View.HotKey
{
    public class GameKeyConfigVM : ViewModel
    {
        private readonly Action _onClose;
        private string _cancelLbl;
        private string _doneLbl;
        private string _resetLbl;
        public MissionLibraryGameKeyOptionCategoryVM GameKeyOptions { get; set; }

        public GameKeyConfigVM(AGameKeyCategoryManager gameKeyCategoryManager, Action<IHotKeySetter> onKeyBindRequest, Action onClose)
        {
            _onClose = onClose;
            GameKeyOptions = new MissionLibraryGameKeyOptionCategoryVM(gameKeyCategoryManager, onKeyBindRequest);
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            GameKeyOptions.RefreshValues();
            CancelLbl = new TextObject("{=3CpNUnVl}Cancel").ToString();
            DoneLbl = new TextObject("{=WiNRdfsm}Done").ToString();
            ResetLbl = new TextObject("{=mAxXKaXp}Reset").ToString();
        }

        [DataSourceProperty]
        public string CancelLbl
        {
            get => _cancelLbl;
            set
            {
                if (value == _cancelLbl)
                    return;
                _cancelLbl = value;
                OnPropertyChangedWithValue(value, nameof(CancelLbl));
            }
        }

        [DataSourceProperty]
        public string DoneLbl
        {
            get => _doneLbl;
            set
            {
                if (value == _doneLbl)
                    return;
                _doneLbl = value;
                OnPropertyChangedWithValue(value, nameof(DoneLbl));
            }
        }

        [DataSourceProperty]
        public string ResetLbl
        {
            get => _resetLbl;
            set
            {
                if (value == _resetLbl)
                    return;
                _resetLbl = value;
                OnPropertyChangedWithValue(value, nameof(ResetLbl));
            }
        }
        protected void ExecuteDone()
        {
            GameKeyOptions.OnDone();
            _onClose?.Invoke();
        }

        public void ExecuteCancel()
        {
            _onClose?.Invoke();
        }
        protected void ExecuteReset()
        {
            InformationManager.ShowInquiry(new InquiryData("", new TextObject("{=cDzWYQrz}Reset to default settings?").ToString(), true, true, new TextObject("{=oHaWR73d}Ok").ToString(), new TextObject("{=3CpNUnVl}Cancel").ToString(), OnResetToDefaults, null));
        }
        private void OnResetToDefaults()
        {
           GameKeyOptions.OnReset();
        }
    }
}
