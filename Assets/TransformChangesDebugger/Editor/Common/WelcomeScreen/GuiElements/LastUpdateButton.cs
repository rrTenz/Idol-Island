using System;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements
{
    public class LastUpdateButton: ChangeMainViewButton, IMainScreenChanger
    {
        public Action<ProductWelcomeScreenBase> RenderMainScrollViewFn => base.RenderMainScrollViewFn;

        public LastUpdateButton(string text, Action<ProductWelcomeScreenBase> renderMainScrollViewFn) 
            : base(text, renderMainScrollViewFn)
        {
        }
    }
}