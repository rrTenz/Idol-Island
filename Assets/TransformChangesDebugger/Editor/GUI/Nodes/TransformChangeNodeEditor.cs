using ImmersiveVRTools.Editor.Common.Utilities;
using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;
using UnityEngine;

namespace TransformChangesDebugger.Editor.GUI.Nodes
{
    [CustomNodeEditor(typeof(TransformChangeNode))]
    internal class TransformChangeNodeEditor : NodeEditor
    {
        private static readonly Color DefaultColor = new Color32(90, 97, 105, 255);
        private TransformChangeNode node;
        
        public override void OnBodyGUI()
        {
            if (node == null)
            {
                node = target as TransformChangeNode;
            }
            
            base.OnBodyGUI();

            GUILayout.Space(5);
            node.IsOriginalMethodCallFoldoutExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(node.IsOriginalMethodCallFoldoutExpanded, "Original Method Call");
            if (node.IsOriginalMethodCallFoldoutExpanded)
            {
                EditorGUILayout.TextField("Name", node.OriginalMethodCall.Name);
                GUILayout.Label("Method Arguments");
                foreach (var arg in node.OriginalMethodCall.Arguments)
                {
                    EditorGUILayout.TextField(arg.GetType().Name, arg.ToString());
                }
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public override Color GetTint()
        {
            if (node != null && node.ComparisonOnlyTransformModifier != null && TransformChangesDebuggerGuiManager.TransformModifierToUseColorMap.TryGetValue(
                node.ComparisonOnlyTransformModifier, out var colorToUse))
            {
                return colorToUse; 
            }
            
            return DefaultColor;
        }

        public override void AddContextMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Goto code"), false, () => CodeEditorManager.GotoScript(node.Originator, node.MethodName));
            //nothing in context menu for now
        }
    }
}