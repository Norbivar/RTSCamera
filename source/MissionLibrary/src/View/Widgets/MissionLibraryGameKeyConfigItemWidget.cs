using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace MissionLibrary.View.Widgets
{
    public class MissionLibraryGameKeyConfigItemWidget : ListPanel
    {
        private MissionLibraryGameKeyConfigWidget _screenWidget;
        private bool _eventsRegistered;

        public MissionLibraryGameKeyConfigItemWidget(UIContext context)
          : base(context)
        {
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
            if (_screenWidget == null)
                _screenWidget = EventManager.Root.GetChild(0).FindChild("Options") as MissionLibraryGameKeyConfigWidget;
            if (_eventsRegistered)
                return;
            RegisterHoverEvents();
            _eventsRegistered = true;
        }

        protected override void OnHoverBegin()
        {
            base.OnHoverBegin();
            SetCurrentOption(false, false);
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();
            ResetCurrentOption();
        }

        private void SetCurrentOption(
          bool fromHoverOverDropdown,
          bool fromBooleanSelection,
          int hoverDropdownItemIndex = -1)
        {
            _screenWidget?.SetCurrentOption(this, null);
        }

        private void ResetCurrentOption() => _screenWidget?.SetCurrentOption(null, null);

        private void RegisterHoverEvents()
        {
            foreach (Widget allChild in AllChildren)
                allChild.PropertyChanged += Child_PropertyChanged;
        }

        private void Child_PropertyChanged(
          PropertyOwnerObject childWidget,
          string propertyName,
          object propertyValue)
        {
            if (propertyName != "IsHovered")
                return;
            if ((bool)propertyValue)
                SetCurrentOption(false, false);
            else
                ResetCurrentOption();
        }

        public string OptionTitle { get; set; }

        public string OptionDescription { get; set; }
    }
}
