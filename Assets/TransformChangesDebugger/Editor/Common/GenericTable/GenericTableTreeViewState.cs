using UnityEditor.IMGUI.Controls;

namespace ImmersiveVRTools.Editor.Common.GenericTable
{
    public class GenericTableTreeViewState
    {
        public TreeViewState TreeViewState { get; }
        public MultiColumnHeaderState MultiColumnHeaderState { get; }

        public GenericTableTreeViewState(TreeViewState treeViewState, MultiColumnHeaderState multiColumnHeaderState)
        {
            TreeViewState = treeViewState;
            MultiColumnHeaderState = multiColumnHeaderState;
        }
    }
}