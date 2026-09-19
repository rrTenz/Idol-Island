namespace TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons
{
    public abstract class BarEditorWindowAddon : NodeEditorWindowAddon
    {
        public BarEditorWindowAddon(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        public BarEditorWindowAddon(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, bool fitHeightToContent) : base(nodeEditorWindowAddonAttachment, width, fitHeightToContent)
        {
        }
    }
}