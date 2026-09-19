using UnityEngine;

namespace TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons
{
    public abstract class NodeEditorWindowAddon
    {
        public NodeEditorWindowAddonAttachment NodeEditorWindowAddonAttachment { get; }
        public int Width { get; private set; }
        public int Height { get; }

        private GUIStyle _windowStyle;
        public virtual GUIStyle WindowStyle
        {
            get
            {
                if (_windowStyle == null)
                {
                    _windowStyle = new GUIStyle(UnityEngine.GUI.skin.textArea);
                }

                return _windowStyle;
            }
        }

        public Rect LastOriginalScreenPosition { get; private set; }
        public Rect LastFinalLayoutRenderArea { get; private set; }
        public bool FitHeightToContent { get; private set; }

        //This is usually required for top / bottom as they'll adjust their width based on screen size and side panels widths
        public void AdjustWidthToActualSize(int width)
        {
            Width = width;
        }

        public NodeEditorWindowAddon(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height)
        {
            NodeEditorWindowAddonAttachment = nodeEditorWindowAddonAttachment;
            Width = width;
            Height = height;
        }
        
        public NodeEditorWindowAddon(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, bool fitHeightToContent)
            : this(nodeEditorWindowAddonAttachment, width, 0)

        {
            FitHeightToContent = fitHeightToContent;
        }

        protected abstract void DrawInternal();

        public void Draw(Rect originalScreenPosition)
        {
            LastOriginalScreenPosition = originalScreenPosition;
            
            DrawInternal();

            if (Event.current.type == EventType.Repaint)
            {
                LastFinalLayoutRenderArea = GUILayoutUtility.GetLastRect();
            }
        }

        public virtual bool ShouldRender()
        {
            return true;
        }
    }
    
    public enum NodeEditorWindowAddonAttachment
    {
        Left,
        Top,
        Right,
        Bottom
    }
}