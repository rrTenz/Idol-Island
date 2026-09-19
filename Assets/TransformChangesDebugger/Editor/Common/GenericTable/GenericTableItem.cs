using UnityEditor.IMGUI.Controls;

namespace ImmersiveVRTools.Editor.Common.GenericTable
{
    public class GenericTableItem<T> : TreeViewItem
    {
        public T Data { get; set; }

        public GenericTableItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
        {
            this.Data = data;
        }
    }
}