using MissionLibrary.View;
using MissionSharedLibrary.View.HotKey;
using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace MissionSharedLibrary.View
{
    public class MenuManager : AMenuManager
    {
        public override IMenuClassCollection MenuClassCollection { get; } = new MenuClassCollection();
        public override MissionView CreateMenuView()
        {
            return new OptionView(24, new Version(1, 1, 0));
        }

        public override MissionView CreateGameKeyConfigView()
        {
            return new GameKeyConfigView();
        }

        public override void RequestToOpenMenu()
        {
            Mission.Current.GetMissionBehavior<OptionView>()?.ActivateMenu();
        }

        public override void RequestToCloseMenu()
        {
            Mission.Current.GetMissionBehavior<OptionView>()?.DeactivateMenu();
        }
    }
}
