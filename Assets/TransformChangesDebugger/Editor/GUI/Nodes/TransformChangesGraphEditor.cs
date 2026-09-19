using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;

namespace TransformChangesDebugger.Editor.GUI.Nodes
{
    [CustomNodeGraphEditor(typeof(TransformChangesGraph))]
    internal class TransformChangesGraphEditor : NodeGraphEditor {
        public override string GetNodeMenuName(System.Type type)
        {
            return null;
        }

        public override void AddContextMenuItems(GenericMenu menu)
        {
            //this disabled existing options from context menu
        }
    }
}