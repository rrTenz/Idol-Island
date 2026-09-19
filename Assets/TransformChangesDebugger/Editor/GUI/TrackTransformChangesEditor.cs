using TransformChangesDebugger.API;
using TransformChangesDebugger.Editor.XNodeExtension;
using TransformChangesDebugger.Runtime;
using UnityEditor;
using UnityEngine;

namespace TransformChangesDebugger.Editor.GUI
{
    [CustomEditor(typeof(TrackTransformChanges))]
    public class TrackTransformChangesEditor: UnityEditor.Editor 
    {
        public override void OnInspectorGUI()
        {
            var trackTransformChanges = target as TrackTransformChanges;
            var isObjectTrackedWithAnyChanges = TransformChangesDebuggerGuiManager.AllTrackedObjects.Contains(trackTransformChanges);
            
            GUILayout.Space(10);
            
            if (TransformChangesDebuggerGuiManager.CurrentNodeEditorWindow)
            {
                var isAlreadySelected = TransformChangesDebuggerGuiManager.SelectedTrackedObject == trackTransformChanges;
                using (new EditorGUI.DisabledScope(!isObjectTrackedWithAnyChanges || isAlreadySelected))
                {
                    if (GUILayout.Button(isAlreadySelected ? "Object changes already in focus, look into tool window" : 
                        isObjectTrackedWithAnyChanges ? "View Changes" : "No changes to view", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
                    {
                        ExtendedNodeEditorWindow.Open();
                        TransformChangesDebuggerGuiManager.SelectedTrackedObject = trackTransformChanges;
                        EditorApplication.isPaused = true;
                    }
                }

            }
            else if (!TransformChangesDebuggerGuiManager.CurrentNodeEditorWindow)
            {
                if (GUILayout.Button("Open Visual Transform Changes Debugger Window", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
                {
                    ExtendedNodeEditorWindow.Open();
                }
            }
            
            GUILayout.Space(10);
            
            base.OnInspectorGUI();
        }
    }
}