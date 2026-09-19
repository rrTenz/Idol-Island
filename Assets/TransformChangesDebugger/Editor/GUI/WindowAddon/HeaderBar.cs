using System;
using System.Collections.Generic;
using ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs;
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
    internal class HeaderBar : BarEditorWindowAddon
    {
        private static readonly string TrackingDisabledMessage = "Tracking is disabled, no changes will be captured and no assemblies will be processed. " +
                                                                 "You can enable it via grey record button in top left corner. You need to pause / play after changing.";

        private static readonly string PreviewModeEnabledMessage =
            "You're in changes preview mode, click on a node and object in the scene will have it's values adjusted to reflect it's state at that time.";
        
        private static readonly int FrameCountLabelWidth = 33;
        
        private bool _isTrackingEnabled = true;

        private EditorPrefsAutoPersistingEnumToIntDictionaryMap _userChosenShowFrames;
        private float _showNextNFramesMin;
        private float _showNextNFramesMax;
        
        //TODO: refactor colored gui button styles creation into helper class
        private static GUIStyle OrangeButton => CachedGUIStyle.GetOrCreate($"{nameof(HeaderBar)}.OrangeButton", () =>
        {
            var style = new GUIStyle(EditorStyles.toolbarButton);
            style.normal.background = TextureHelper.CreateGuiBackgroundColor(TransformChangesDebuggerStyles.OrangeColor);
            return style;
        });

        private EditorPrefsAutoPersistingEnumToIntDictionaryMap UserChosenShowFrames
        {
            get
            {
                if (_userChosenShowFrames == null)
                {
                    _userChosenShowFrames = new EditorPrefsAutoPersistingEnumToIntDictionaryMap(
                        TransformChangesDebuggerEditorPrefs.UserChosenShowFrames,
                        new Dictionary<int, int>()
                        {
                            [(int)ShowFramesPerfValue.Min] = 0,
                            [(int)ShowFramesPerfValue.Max] = 20
                        }
                    );
                }

                return _userChosenShowFrames;
            }
        }

        public HeaderBar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        protected override void DrawInternal()
        {
            GUILayout.BeginHorizontal();

            TransformChangesDebuggerGuiManager.IsTrackingEnabled  = GUILayout.Toggle(TransformChangesDebuggerGuiManager.IsTrackingEnabled, TransformChangesDebuggerGuiManager.IsTrackingEnabled ? TransformChangesDebuggerStyles.TrackChangesOn : TransformChangesDebuggerStyles.TrackChangesOff, EditorStyles.toolbarButton);
            TransformChangesDebuggerGuiManager.ToggleMessageToUser(!TransformChangesDebuggerGuiManager.IsTrackingEnabled, TrackingDisabledMessage);

            var areUserControlsEnabled = TransformChangesDebuggerGuiManager.SelectedTrackedObject;
            
            using (new EditorGUI.DisabledScope(!areUserControlsEnabled))
            {
                var currentStartFrameIndex = TransformChangesTracker.GetNewestFrameNumberWithTrackedChanges(UserChosenShowFrames[(int)ShowFramesPerfValue.Min]);
                var isViewOutOfDate = TransformChangesDebuggerGuiManager.SelectedFrame != 0 && currentStartFrameIndex != TransformChangesDebuggerGuiManager.SelectedFrame;
                TransformChangesDebuggerGuiManager.ToggleMessageToUser(isViewOutOfDate, "View is out of date, press refresh in top bar to update");

                if (GUILayout.Button("Refresh", isViewOutOfDate ?  OrangeButton : EditorStyles.toolbarButton))
                {
                    if (Application.isPlaying) EditorApplication.isPaused = true;

                    TransformChangesDebuggerGuiManager.SelectedFrame = currentStartFrameIndex;
                }
            }
            
            using (new EditorGUI.DisabledScope(!areUserControlsEnabled))
            {
                var previousShowNextNFramesMin = _showNextNFramesMin = UserChosenShowFrames[(int)ShowFramesPerfValue.Min];
                var previousShowNextNFramesMax = _showNextNFramesMax = UserChosenShowFrames[(int)ShowFramesPerfValue.Max];
                
                var showingMaxFrames = _showNextNFramesMax - _showNextNFramesMin >= TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen;
                GUILayout.Label($"Showing Frames: {_showNextNFramesMax - _showNextNFramesMin} " +
                                $"{(showingMaxFrames ? "[MAX]" : "")}", 
                    TransformChangesDebuggerStyles.EmphasisedText, GUILayout.Width(170)
                );
                
                TransformChangesDebuggerStyles.AddHelperTooltip($"Use frame slider to choose how many frames to skip (since last one captured) [left side of the slider]." +
                                                                $"\n\nAnd how far in the past to go [right side of the slider]." +
                                                                $"\n\nFor performance reasons you can see up to {TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen} at the same time, you can adjust that from settings window.", new Vector4(0, 3, 0, 0));

                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label($"[{_showNextNFramesMin.ToString()}]", GUILayout.Width(FrameCountLabelWidth));
                
                EditorGUILayout.MinMaxSlider(ref _showNextNFramesMin, ref _showNextNFramesMax, 
                    0, 
                    Mathf.Max(_showNextNFramesMax, TransformChangesDebuggerGuiManager.AllAvailableTrackedDataFrameCount)
                );
                //adjust values to make sure they don't go over max allowed on screen
                if (_showNextNFramesMax - _showNextNFramesMin > TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen)
                {
                    _showNextNFramesMin = previousShowNextNFramesMin;
                    _showNextNFramesMax = _showNextNFramesMin + TransformChangesDebuggerGuiManager.MaxAllowedFramesToShowOnScreen;
                }
                
                UserChosenShowFrames[(int)ShowFramesPerfValue.Min] = (int)_showNextNFramesMin;
                UserChosenShowFrames[(int)ShowFramesPerfValue.Max] = (int)_showNextNFramesMax;

                if ((int)previousShowNextNFramesMin != (int)_showNextNFramesMin || (int)previousShowNextNFramesMax != (int)_showNextNFramesMax)
                {
                    if (Application.isPlaying) EditorApplication.isPaused = true;
                    TransformChangesDebuggerGuiManager.SkipNFramesFromLastCaptured = UserChosenShowFrames[(int)ShowFramesPerfValue.Min];
                    TransformChangesDebuggerGuiManager.ShowNFrames = UserChosenShowFrames[(int)ShowFramesPerfValue.Max] - UserChosenShowFrames[(int)ShowFramesPerfValue.Min];
                }
                
                GUILayout.Label($"[{_showNextNFramesMax.ToString()}]", GUILayout.Width(FrameCountLabelWidth));

                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Position] = GUILayout.Toggle(TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Position], "position", GUILayout.Width(70));
            TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Rotation] = GUILayout.Toggle(TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Rotation], "rotation", GUILayout.Width(70));
            TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Scale] = GUILayout.Toggle(TransformChangesDebuggerGuiManager.UserChosenShowTransformChangesFor[(int)ShowTransformChangesFor.Scale], "scale", GUILayout.Width(70));
            TransformChangesDebuggerStyles.AddHelperTooltip($"You can use options to the left to focus on specific type of changes.", new Vector4(0, 3, 0, 0));

            using (new EditorGUI.DisabledScope(!areUserControlsEnabled || !Application.isPlaying))
            {
                TransformChangesDebuggerGuiManager.IsHistoryPreviewEnabled =  GUILayout.Toggle(TransformChangesDebuggerGuiManager.IsHistoryPreviewEnabled, "preview", GUILayout.Width(70));
                TransformChangesDebuggerGuiManager.ToggleMessageToUser(
                    TransformChangesDebuggerGuiManager.IsHistoryPreviewEnabled && Application.isPlaying && areUserControlsEnabled, 
                    PreviewModeEnabledMessage
                );
            }
            
            GUILayout.EndHorizontal();
        }
    }

    public enum ShowFramesPerfValue
    {
        Min,
        Max
    }
    
    public enum ShowTransformChangesFor {
        Position,
        Rotation,
        Scale
    }
}