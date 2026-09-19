using System;
using UnityEditor.SceneManagement;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.GuiElements
{
    public class LaunchSceneButton : ClickableElement, IMainScreenChanger
    {
        private readonly Func<ProductWelcomeScreenBase, string> _getScenePath;
        public Action<ProductWelcomeScreenBase> RenderMainScrollViewFn { get; }

        public LaunchSceneButton(string text, Func<ProductWelcomeScreenBase, string> getScenePath, Action<ProductWelcomeScreenBase> renderMainScrollViewFn = null)
            : base(text, "BuildSettings.Editor.Small")
        {
            _getScenePath = getScenePath;
            RenderMainScrollViewFn = renderMainScrollViewFn;
        }

        public override void OnClick(ProductWelcomeScreenBase welcomeScreen)
        {
            var scenePath = _getScenePath(welcomeScreen);
            EditorSceneManager.OpenScene(scenePath);
            
            if(RenderMainScrollViewFn != null) 
                welcomeScreen.ChangeMainScrollViewRenderFn(RenderMainScrollViewFn, Text);
        }
    }
}