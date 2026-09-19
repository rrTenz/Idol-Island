using System;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements
{
    public class ScrollViewGuiSection : GuiSectionBase
    {
        public Action<ProductWelcomeScreenBase> RenderMainScrollViewSection { get; }

        public ScrollViewGuiSection(string labelText, Action<ProductWelcomeScreenBase> renderMainScrollViewSection)
            : base(labelText)
        {
            RenderMainScrollViewSection = renderMainScrollViewSection;
        }
    }
}