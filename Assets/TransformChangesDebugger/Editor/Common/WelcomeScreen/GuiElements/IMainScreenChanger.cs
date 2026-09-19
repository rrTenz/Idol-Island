using System;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements
{
    public interface IMainScreenChanger
    {
        string Text { get; }
        Action<ProductWelcomeScreenBase> RenderMainScrollViewFn { get; }
    }
}