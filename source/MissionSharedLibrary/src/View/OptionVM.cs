using MissionLibrary.View;
using MissionSharedLibrary.View.HotKey;
using MissionSharedLibrary.View.ViewModelCollection.Basic;
using System;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace MissionSharedLibrary.View
{
    public class OptionVM : MissionMenuVMBase
    {
        private readonly IMenuClassCollection _menuClassCollection;

        public OptionVM(IMenuClassCollection menuClassCollection, Action closeMenu)
            : base(closeMenu)
        {
            _menuClassCollection = menuClassCollection;
            OptionClassCollection = menuClassCollection.GetViewModel();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            ConfigKeyTitle.RefreshValues();
            ConfigKeyHint.RefreshValues();
            OptionClassCollection.RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            _menuClassCollection.Clear();
        }

        public TextViewModel ConfigKeyTitle { get; set; } =
            new TextViewModel(GameTexts.FindText("str_mission_library_gamekey_config"));

        public HintViewModel ConfigKeyHint { get; set; } =
            new HintViewModel(GameTexts.FindText("str_mission_library_config_key_hint"));

        private readonly GameKeyConfigView _gameKeyConfigView = Mission.Current.GetMissionBehavior<GameKeyConfigView>();

        public void ConfigKey()
        {
            InformationManager.HideInquiry();
            _gameKeyConfigView?.Activate();
        }

        public ViewModel OptionClassCollection { get; }
    }
}
