using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.TwoDimension;

namespace MissionLibrary.View.Widgets
{
    public class MissionLibraryGameKeyConfigWidget : Widget
    {
        private Widget _currentOptionWidget;

        public RichTextWidget CurrentOptionDescriptionWidget { get; set; }

        public RichTextWidget CurrentOptionNameWidget { get; set; }

        public Widget CurrentOptionImageWidget { get; set; }
        public MissionLibraryGameKeyConfigWidget(UIContext context)
          : base(context)
        {
        }
        

        public void SetCurrentOption(Widget currentOptionWidget, Sprite newGraphicsSprite)
        {
            if (_currentOptionWidget != currentOptionWidget)
            {
                _currentOptionWidget = currentOptionWidget;
                string str1 = "";
                string str2 = "";
                if (_currentOptionWidget is MissionLibraryGameKeyConfigItemWidget currentOptionWidget1)
                {
                    str1 = currentOptionWidget1.OptionDescription;
                    str2 = currentOptionWidget1.OptionTitle;
                }
                if (CurrentOptionDescriptionWidget != null)
                    CurrentOptionDescriptionWidget.Text = str1;
                if (CurrentOptionDescriptionWidget != null)
                    CurrentOptionNameWidget.Text = str2;
            }
            if (CurrentOptionImageWidget == null || CurrentOptionImageWidget.Sprite == newGraphicsSprite)
                return;
            CurrentOptionImageWidget.Sprite = newGraphicsSprite;
            if (newGraphicsSprite == null)
                return;
            float num = this.CurrentOptionImageWidget.SuggestedWidth / (float)newGraphicsSprite.Width;
            this.CurrentOptionImageWidget.SuggestedHeight = (float)newGraphicsSprite.Height * num;
        }
    }
}
