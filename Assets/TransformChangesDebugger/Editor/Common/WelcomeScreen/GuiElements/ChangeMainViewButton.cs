using System;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements
{
    public class ChangeMainViewButton : ClickableElement, IMainScreenChanger
    {
        public Action<ProductWelcomeScreenBase> RenderMainScrollViewFn { get; }

        public ChangeMainViewButton(string text, Action<ProductWelcomeScreenBase> renderMainScrollViewFn)
            : base(text, string.Empty)
        {
            RenderMainScrollViewFn = renderMainScrollViewFn;
        }

        public override void OnClick(ProductWelcomeScreenBase welcomeScreen)
        {
            welcomeScreen.ChangeMainScrollViewRenderFn(RenderMainScrollViewFn, Text);
        }
    }
}