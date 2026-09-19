using System.Linq;
using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;
using UnityEngine;

namespace TransformChangesDebugger.Editor.GUI.Nodes
{
    [CustomNodeEditor(typeof(TransformChangesDebuggerNodeBase))]
    internal class TransformChangesDebuggerNodeBaseEditor : NodeEditor {
        
        private TransformChangesDebuggerNodeBase node;
        // private LogicGraphEditor graphEditor;

        public override void OnHeaderGUI()
        {
            if (node == null)
            {
                node = target as TransformChangesDebuggerNodeBase;
                // graphEditor = NodeGraphEditor.GetEditor(target.graph, window) as LogicGraphEditor;
            }

            base.OnHeaderGUI();
        }

        public override void OnBodyGUI()
        {
            // Unity specifically requires this to save/update any serial object.
            // serializedObject.Update(); must go at the start of an inspector gui, and
            // serializedObject.ApplyModifiedProperties(); goes at the end.
            serializedObject.Update();
            string[] excludes = { "m_Script", "graph", "position", "ports" };

            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren)) {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                // UnityEngine.GUI.enabled = false; //TODO: not ideal as text also turns greyinsh, other than that quite good user can't change values but they can for example select objects
                EditorGUILayout.PropertyField(iterator, null, true, GUILayout.MinWidth(30));
                // UnityEngine.GUI.enabled = true;
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        public override void AddContextMenuItems(GenericMenu menu)
        {
            //nothing in context menu for now
        }
    }
}