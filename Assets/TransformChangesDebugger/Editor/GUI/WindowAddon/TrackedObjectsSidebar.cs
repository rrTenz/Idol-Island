using System.Linq;
using ImmersiveVRTools.Editor.Common.Utilities;
using ImmersiveVRTools.Runtime.Common.Utilities;
using TransformChangesDebugger.API;
using TransformChangesDebugger.Editor;
using TransformChangesDebugger.Editor.GUI;
using TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor
{
    internal class TrackedObjectsSidebar : ScrollableBarEditorWindowAddon
    {
        private static readonly string HelpText = $"All transform changes made to GameObjects with '{nameof(TrackTransformChanges)}' script will be tracked." +
                                  "\n\nSelect desired object from the list below." +
                                  $"\n\nIf none are visible - make sure '{nameof(TrackTransformChanges)}' was added to desired scripts" +
                                  $"\n\nIn case they still don't show it means tool was not able to capture changes - please have a look in 'Assemblies' tab to make sure the code that can potentially make changes is tracked." +
                                  $"\n\nIf all that fails please get in touch.";
        
        private static readonly string NothingTrackedMessage = "No objects tracked. Assemblies will not be processed to save time. Add 'TrackTransformChanges' to game object that you wish to track and restart play mode.";

        private static GUIStyle SelectedGuiStyle => CachedGUIStyle.GetOrCreate($"{nameof(TrackedObjectsSidebar)}.{nameof(SelectedGuiStyle)}", () =>
        {
            var style = new GUIStyle(GUI.skin.button);
            style.normal.background = TextureHelper.CreateGuiBackgroundColor(new Color(0.0f, 1f, 0.0f, 0.6f));
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            return style;
        });

        public TrackedObjectsSidebar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        protected override void DrawInternal()
        {
            var showNoChangesRecordedMessage = !TransformChangesTracker.HasAnyChanges() && TransformChangesDebuggerGuiManager.IsTrackingEnabled && (!Application.isPlaying || EditorApplication.isPaused);
            TransformChangesDebuggerGuiManager.ToggleMessageToUser(showNoChangesRecordedMessage, "No changes recorded, press play to start tracking");
            TransformChangesDebuggerGuiManager.ToggleMessageToUser(TransformChangesTracker.HasAnyChanges() && !TransformChangesDebuggerGuiManager.SelectedTrackedObject, 
                "Select tracked object from the left to show changes");
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Tracked Objects", TransformChangesDebuggerStyles.SidebarHeader);
            TransformChangesDebuggerStyles.AddHelperTooltip(HelpText, new Vector4(0, 9, 0 , 0));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            var selectedGo = Selection.activeGameObject;
            var trackTransformChangesResolved = false;
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying || !selectedGo || (trackTransformChangesResolved = selectedGo.GetComponent<TrackTransformChanges>())))
            {
                var secondLine = Application.isPlaying ? "Exit playmode first \nor add component manually"
                    : selectedGo == null ? "Select in editor first"
                    : trackTransformChangesResolved ? "Already tracked"
                    : selectedGo.name;
                if (GUILayout.Button($"Track currently selected object\n({secondLine})"))
                {
                    selectedGo.AddComponent<TrackTransformChanges>();
                }
            }
            
            GUILayout.Space(20);
            
            if (!TransformChangesDebuggerGuiManager.AllTrackedObjects.Any())
            {

                GUILayout.Label($"Add '{nameof(TrackTransformChanges)}' script to game objects to start tracking. Script needs to be present before application is in play mode.", 
                    TransformChangesDebuggerStyles.EmphasisedText);
                
                GUILayout.Label("\n\n\n" + HelpText, TransformChangesDebuggerStyles.WrapText , GUILayout.Width(Width - 10));
                return;
            }
            
            foreach (var trackedObject in TransformChangesDebuggerGuiManager.AllTrackedObjects.Where(t => t))
            {
                if (GUILayout.Button(trackedObject.name, TransformChangesDebuggerGuiManager.SelectedTrackedObject == trackedObject ? SelectedGuiStyle : GUI.skin.button))
                {
                    if (!EditorApplication.isPaused) EditorApplication.isPaused = true;
                    TransformChangesDebuggerGuiManager.SelectedTrackedObject = trackedObject;
                    TransformChangesDebuggerGuiManager.NavigateToFirstNode();
                }
            }
        }
    }
}