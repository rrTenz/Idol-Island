using UnityEngine;

namespace TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons
{
    public abstract class ScrollableBarEditorWindowAddon: NodeEditorWindowAddon
    {
        private Vector2 _scrollViewPosition = new Vector2();
        
        public ScrollableBarEditorWindowAddon(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        public void BeginVerticalScrollView(float windowHeight, float contentHeight)
        {
            var fullContentsSize = new Rect(0f, 0f, Width, contentHeight);
            _scrollViewPosition = UnityEngine.GUI.BeginScrollView(new Rect(0f, 0f, Width, windowHeight), 
                _scrollViewPosition, 
                fullContentsSize, 
                GUIStyle.none, 
                UnityEngine.GUI.skin.verticalScrollbar
            );
        }
    }
}