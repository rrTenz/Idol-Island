using TransformChangesDebugger.API.Patches;
using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;
using UnityEngine;

namespace TransformChangesDebugger.Editor.GUI.Nodes
{
    [CustomNodeEditor(typeof(ChangeMismatchNode))]
    internal class ChangeMismatchNodeEditor : NodeEditor
    {
        //TODO: remove workaround note once fixed
        private static readonly string WorkaroundMethodsChangingBothRotationAndPosition = "\n\nIf your using methods that modify more than one value, eg. SetPositionAndRotation - only position change is recorded. This rotation mismatch could be ignored.";
        private static readonly string WarningText = "This indicates that value at the last captured change is different to next change's value before execution. This likely means that some changes are made from assembly that's not yet patched." +
                                                     "\n\nPlease use bottom 'Assemblies' panel to adjust and retry." +
                                                     "\n\nIf you're still seeing that then perhaps change is being made via a method that's not yet captured by the tool, eg rigidbody / physics.";
        
        private ChangeMismatchNode node;

        public override void OnBodyGUI()
        {
            base.OnBodyGUI();

            if (node.RenderWarningAsLabel)
            {
                GUILayout.Label(GenerateWarningText(node), TransformChangesDebuggerStyles.WrapText);
                GUILayout.Space(20);
            }

            EditorGUILayout.TextField("Expected Value", node.ExpectedValue.ToString());
            EditorGUILayout.TextField("Actual Value", node.ActualValue.ToString());
        }

        public override void OnHeaderGUI()
        {            
            if (node == null)
            {
                node = target as ChangeMismatchNode;
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30), GUILayout.ExpandWidth(true));
            if (!node.RenderWarningAsLabel)
            {
                TransformChangesDebuggerStyles.AddHelperTooltip(GenerateWarningText(node), new Vector4(0, 7, 0, 0));
            }
            GUILayout.EndHorizontal();
        }
        
        private static string GenerateWarningText(ChangeMismatchNode changeMismatchNode)
        {
            return WarningText + (changeMismatchNode.ChangeType == ChangeType.Rotation ? WorkaroundMethodsChangingBothRotationAndPosition : string.Empty);
        }

        public override Color GetTint()
        {
            return TransformChangesDebuggerStyles.OrangeColor;
        }
    }
}