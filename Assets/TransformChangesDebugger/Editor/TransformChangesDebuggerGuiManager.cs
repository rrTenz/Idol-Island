using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs;
using TransformChangesDebugger.API;
using TransformChangesDebugger.API.Patches;
using TransformChangesDebugger.Editor.XNode;
using TransformChangesDebugger.Editor.XNodeExtension;
using TransformChangesDebugger.Runtime;
using TransformChangesDebugger.Runtime.GUI;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace TransformChangesDebugger.Editor
{
    /// <summary>
    /// This class controls how the GUI behaves with various options
    /// </summary>
    public class TransformChangesDebuggerGuiManager
    {
        /// <summary>
        /// Fired when some significant change happened on main graph and signals to GUI it should reevaluate what's on screen
        /// </summary>
        public static event EventHandler<GraphDrawingDependedSettingChangedEventArgs> GraphDrawingDependentSettingChanged;

        private static EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap _transformModifierToUseColorMap;
        internal static EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap TransformModifierToUseColorMap
        {
            get
            {
                if (_transformModifierToUseColorMap == null)
                {
                    _transformModifierToUseColorMap = new EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap(
                        TransformChangesDebuggerEditorPrefs.TransformModifierToUseColorMap,
                        new Dictionary<ISerializableTransformModifier, Color>()
                    );
                }

                return _transformModifierToUseColorMap;
            }
        }

        internal static ExtendedNodeEditorWindow CurrentNodeEditorWindow
        {
            get => _currentNodeEditorWindow;
            private set => _currentNodeEditorWindow = value;
        }

        private static TransformChangesDebuggerNodeBase CurrentlySelectedNode  { get; set; }
        
        private static EditorPrefsAutoPersistingEnumToBoolDictionaryMap _generalSettings;

        private static EditorPrefsAutoPersistingEnumToBoolDictionaryMap GeneralSettings
        {
            get
            {
                if (_generalSettings == null)
                {
                    _generalSettings = new EditorPrefsAutoPersistingEnumToBoolDictionaryMap(
                        TransformChangesDebuggerEditorPrefs.GeneralFlagSettings,
                        new Dictionary<int, bool>()
                        {
                            [(int)GeneralFlagSetting.IsTrackingEnabled] = true,
                            [(int)GeneralFlagSetting.IsHistoryPreviewEnabled] = true,
                        }
                    );
                }

                return _generalSettings;
            }
        }

        /// <summary>
        /// Controls tracking for whole tool, if disabled no assemblies will be patched. Convenience property - you should be setting TransformChangesDebuggerManager.IsTrackingEnabled instead
        /// </summary>
        public static bool IsTrackingEnabled
        {
            get => GeneralSettings[(int)GeneralFlagSetting.IsTrackingEnabled];
            set
            {
                GeneralSettings[(int)GeneralFlagSetting.IsTrackingEnabled] = value;
                TransformChangesDebuggerManager.IsTrackingEnabled = value;
            }
        }
        
        /// <summary>
        /// Controls if currently selected change is going to be applied to game object in pause mode (preview changes)
        /// </summary>
        public static bool IsHistoryPreviewEnabled
        {
            get => GeneralSettings[(int)GeneralFlagSetting.IsHistoryPreviewEnabled];
            set => GeneralSettings[(int)GeneralFlagSetting.IsHistoryPreviewEnabled] = value;
        }
        
        private static TrackTransformChanges _selectedTrackedObject;
        /// <summary>
        /// Controls which object changes are currently visible on main screen
        /// </summary>
        public static TrackTransformChanges SelectedTrackedObject
        {
            get => _selectedTrackedObject;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _selectedTrackedObject != value, 
                (v) => _selectedTrackedObject = v, 
                () => Selection.activeGameObject = value.gameObject); 
            }
        }

        private static int _selectedFrame;
        internal static int SelectedFrame
        {
            get => _selectedFrame;
            
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _selectedFrame != value, 
                (v) => _selectedFrame = v); 
            }
        }

        private static int _skipNFramesFromLastCaptured;
        /// <summary>
        /// Controls how many frames should be skipped since last captured one to be displayed on screen. This works together with <see cref="ShowNFrames"/> to show changes from frame range
        /// </summary>
        public static int SkipNFramesFromLastCaptured
        {
            get => _skipNFramesFromLastCaptured;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _skipNFramesFromLastCaptured != value, 
                (v) => _skipNFramesFromLastCaptured = v, 
                () => SelectedFrame = TransformChangesTracker.GetNewestFrameNumberWithTrackedChanges(SkipNFramesFromLastCaptured),
                true); 
            }
        }

        private static int _showNFrames = 10;
        /// <summary>
        /// Controls how many frames should be shown on screen.  This works together with <see cref="SkipNFramesFromLastCaptured"/> to show changes from frame range
        /// </summary>
        public static int ShowNFrames
        {
            get => _showNFrames;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _showNFrames != value, 
                (v) => _showNFrames = v); 
            }
        }

        private static bool _showChangesForPosition;
        /// <summary>
        /// Controls whether position related changes should be visible on screen
        /// </summary>
        public static bool ShowChangesForPosition
        {
            get => _showChangesForPosition;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _showChangesForPosition != value, 
                (v) => _showChangesForPosition = v); 
            }
        }
        
        private static bool _showChangesForRotation;
        /// <summary>
        /// Controls whether rotation related changes should be visible on screen
        /// </summary>
        public static bool ShowChangesForRotation
        {
            get => _showChangesForRotation;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _showChangesForRotation != value, 
                (v) => _showChangesForRotation = v); 
            }
        }
        
        private static bool _showChangesForScale;
        /// <summary>
        /// Controls whether scale related changes should be visible on screen
        /// </summary>
        public static bool ShowChangesForScale
        {
            get => _showChangesForScale;
            set { SetPropertyValueAndTriggerUpdateIfDifferent(value, 
                () => _showChangesForScale != value, 
                (v) => _showChangesForScale = v); 
            }
        }
        
        private static void SetPropertyValueAndTriggerUpdateIfDifferent<TValue>(TValue value, Func<bool> isDifferentFn, Action<TValue> setValue, Action onIsDifferent = null, bool skipGuiVisibleDataUpdate = false)
        {
            var isDifferent = isDifferentFn();
            setValue(value);

            if (isDifferent)
            {
                if(!skipGuiVisibleDataUpdate) 
                    UpdateGuiVisibleData();
                
                onIsDifferent?.Invoke();
                
                if(!skipGuiVisibleDataUpdate) 
                    GraphDrawingDependentSettingChanged?.Invoke(null, new GraphDrawingDependedSettingChangedEventArgs(SelectedTrackedObject,
                        SelectedFrameIndexToChangesForSelectedObjectMap, ShowNFrames, ShowChangesForPosition, ShowChangesForRotation, ShowChangesForScale)
                    );
            }
        }

        /// <summary>
        /// All objects that are currently tracked and can be selected in GUI
        /// </summary>
        public static List<TrackTransformChanges> AllTrackedObjects { get; private set; } = new List<TrackTransformChanges>();
        internal static Dictionary<int, List<TransformChange>> SelectedFrameIndexToChangesForSelectedObjectMap { get; private set; } = new Dictionary<int ,List<TransformChange>>();
        internal static Dictionary<string, List<TransformModifier>> TransformModifiersForSelectedObjectAndFrameGroupedByCallingObjectName { get; private set; } = new Dictionary<string, List<TransformModifier>>();
        
        /// <summary>
        /// Information about assemblies that are available for patching or were patched. This also includes performance measurements
        /// </summary>
        public static List<AssemblyPatchInfo> AssemblyPatchInfoEntries { get; set; } = new List<AssemblyPatchInfo>();
        internal static Dictionary<string, List<AssemblyPatchInfo>> UserFriendlyGroupNameToAssemblyPatchInfoEntries { get; set; } = new Dictionary<string, List<AssemblyPatchInfo>>();
        internal static int AllAvailableTrackedDataFrameCount =>  TransformChangesTracker.AllAvailableTrackedDataFrameCount;
        
        /// <summary>
        /// Limit amount of change nodes that can be shown on screen at given time - due to performance but also practical reasons this should be kept reasonable
        /// </summary>
        public static int MaxAllowedFramesToShowOnScreen { get; set; } = 100;
        
        private static int _updateGuiVisibleDataLastRunInFrame;
        
        private static EditorPrefsAutoPersistingEnumToBoolDictionaryMap _showTransformChangesFor;
        private static ExtendedNodeEditorWindow _currentNodeEditorWindow;

        internal static EditorPrefsAutoPersistingEnumToBoolDictionaryMap UserChosenShowTransformChangesFor
        {
            get
            {
                if (_showTransformChangesFor == null)
                {
                    _showTransformChangesFor = new EditorPrefsAutoPersistingEnumToBoolDictionaryMap(
                        TransformChangesDebuggerEditorPrefs.UserChosenShowTransformChangesFor,
                        new Dictionary<int, bool>()
                        {
                            [(int)ShowTransformChangesFor.Position] = true,
                            [(int)ShowTransformChangesFor.Rotation] = true,
                            [(int)ShowTransformChangesFor.Scale] = true
                        },
                        (s, valueChanged) => { SetShowChangesForPositionToPersistedValuesMap(); }
                    );
                }

                return _showTransformChangesFor;
            }
        }

        private static void SetShowChangesForPositionToPersistedValuesMap()
        {
            ShowChangesForPosition = UserChosenShowTransformChangesFor[(int) ShowTransformChangesFor.Position];
            ShowChangesForRotation = UserChosenShowTransformChangesFor[(int) ShowTransformChangesFor.Rotation];
            ShowChangesForScale = UserChosenShowTransformChangesFor[(int) ShowTransformChangesFor.Scale];
        }

        internal static void Initialize(ExtendedNodeEditorWindow nodeEditorWindow)
        {
            CurrentNodeEditorWindow = nodeEditorWindow;
            SetShowChangesForPositionToPersistedValuesMap();
        }
        
        internal static HashSet<string> UserMessages { get; private set; } = new HashSet<string>();

        internal static void ToggleMessageToUser(bool isVisible, string message)
        {
            //adding messages should only happen on repaint, otherwise it'll throw errors as collection can change between event calls
            if (Event.current == null ||  Event.current.type == EventType.Repaint)
            {
                if(isVisible)
                    UserMessages.Add(message);
                else 
                    UserMessages.Remove(message);
            }
        }

        /// <summary>
        /// Move node graph to first visible change node
        /// </summary>
        public static void NavigateToFirstNode()
        {
            var firstChangeNode = CurrentNodeEditorWindow.graph.nodes.FirstOrDefault(n => n is TransformChangeNode);
            if (firstChangeNode)
            {
                //TODO: not quite sure why this needs adjustment to correctly show as when using Prev/Next - it's just aligned as needed - timing?
                CurrentNodeEditorWindow.MoveNodeToView(firstChangeNode, new Vector2(-100, -100));
            }
        }
        
        /// <summary>
        /// Move node graph to next node for same modifier
        /// </summary>
        public static void NavigateToNextNode(TransformModifier doneByTransformModifier) => NavigateToNode(doneByTransformModifier, +1);
        
        /// <summary>
        /// Move node graph to previous node for same modifier
        /// </summary>
        public static void NavigateToPreviousNode(TransformModifier doneByTransformModifier) => NavigateToNode(doneByTransformModifier, -1);
        
        private static void NavigateToNode(TransformModifier doneByTransformModifier, int iteratorAdjustment)
        {
            var nodes = CurrentNodeEditorWindow.graph.nodes;
            var currentNodeIndex = CurrentlySelectedNode ? nodes.IndexOf(CurrentlySelectedNode) : 0;
            for (var i = currentNodeIndex + iteratorAdjustment; iteratorAdjustment > 0 ? i < nodes.Count : i >= 0; i = i + iteratorAdjustment)
            {
                if (nodes[i] is TransformChangeNode transformChangeNode)
                {
                    if (transformChangeNode.IsModifiedVia(doneByTransformModifier))
                    {
                        CurrentNodeEditorWindow.MoveNodeToView(transformChangeNode);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Set background color for change nodes and modifiers to make changes of same type stand out better 
        /// </summary>
        public static void UseColorForChangesDoneViaModifiers(TransformModifier transformModifier, Color color)
        {
            var entryUsingExistingColor = TransformModifierToUseColorMap
                .FirstOrDefault(kv => kv.Value == color);
            
            if (entryUsingExistingColor.Key != null)
                TransformModifierToUseColorMap.Remove(entryUsingExistingColor.Key);

            TransformModifierToUseColorMap[transformModifier] = color;
        }
        
        /// <summary>
        /// Remove background color for change nodes and modifiers
        /// </summary>
        public static void StopUsingColorForChangesDoneViaModifiers(TransformModifier transformModifier)
        {
            var key = (ComparisonOnlyTransformModifier) transformModifier;
            if (TransformModifierToUseColorMap.ContainsKey(key))
                TransformModifierToUseColorMap.Remove(key);
        }

        [MenuItem("Window/Transform Changes Debugger/Main Debugger Window")]
        private static void Init()
        {
            var editorWindow = ExtendedNodeEditorWindow.Open();
            editorWindow.Initialize();
            //HACK: for some reason focus does not always work, even calling .Focus seems to not always work - this will ensure graph editor is validated
            typeof(NodeEditorWindow).GetMethod("ValidateGraphEditor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(editorWindow, new object [0]);
            editorWindow.Show();

            SkipNFramesFromLastCaptured = 0; //on start show last captured frame
            
            //keep is tracking enabled changed in sync, in case user changes directly in code
            TransformChangesDebuggerManager.IsTrackingEnabledChanged += (sender, isTrackingEnabled) =>
            {
                IsTrackingEnabled = isTrackingEnabled;
            };
        }

        internal static void PatchSettersForSelectedAssemblies()
        {
            var selectedAssemblyPaths = AssemblyPatchInfoEntries
                .Where(a => a.IsPatchingEnabled)
                .Select(a => new FileInfo(a.AssemblyFilePath))
                .ToList();
            
            TransformChangesDebuggerManager.EnableChangeTracking(selectedAssemblyPaths);
        }


        internal static void UpdateAssemblyToPatchGuiData(List<RedirectSetterMethodsFromCallingCodeForAssyResult> redirectSetterMethodsFromCallingCodeForAssyResults)
        {
            AssemblyPatchInfoEntries = TransformChangesDebuggerManager.AllUserPatchableAssyPaths.Select(assemblyFile =>
            {
                var existingEntry = AssemblyPatchInfoEntries.FirstOrDefault(assyInfoEntry => assyInfoEntry.AssemblyFilePath == assemblyFile.FullName);
                AssemblyPatchInfo entryToAdjust;
                if (existingEntry == null)
                {
                    entryToAdjust = new AssemblyPatchInfo(assemblyFile.Name, assemblyFile.FullName, 0, 0, 
                        TransformChangesDebuggerInitializer.DefaultAssemblyNamesToPatch.Contains(assemblyFile.Name)
                    );
                }
                else
                {
                    entryToAdjust = existingEntry; 
                }

                var redirectSetterMethodsResultForAssembly = redirectSetterMethodsFromCallingCodeForAssyResults
                    .FirstOrDefault(r => r.AssemblyPath.FullName == assemblyFile.FullName && r.IsPatchAssemblyExecuted);
                if (redirectSetterMethodsResultForAssembly != null)
                {
                    //TODO: show cached times as well to highlight that this time is just one off
                    entryToAdjust.LastPatchedMethodCount = redirectSetterMethodsResultForAssembly.MethodInterceptionParamEntries.Count;
                    entryToAdjust.LastPatchedTimeTaken = redirectSetterMethodsResultForAssembly.TimeTakenToFindMethodsToPatch 
                                                         + redirectSetterMethodsResultForAssembly.TimeTakenToPatchMethods;
                    entryToAdjust.IsPatchingEnabled = true;
                }
                
                return entryToAdjust;
            }).ToList();

            PersistAssemblyPatchInfoEntries();
            
            var userCodeStartsWithEntries = ((string)TransformChangesDebuggerPreference.UserCodeDllsStartWithSemicolonDelimitedPreferenceDefinition.GetEditorPersistedValueOrDefault())
                .Split(new string[] {";"}, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            UserFriendlyGroupNameToAssemblyPatchInfoEntries = AssemblyPatchInfoEntries
                .GroupBy(p =>
                {
                    if (userCodeStartsWithEntries.Any(userCodeStartWith => p.Name.StartsWith(userCodeStartWith))) return UserFriendlyAssemblyGroupingKeys.UserCode;
                    if (p.Name.StartsWith("Unity")) return UserFriendlyAssemblyGroupingKeys.Unity;
                    return UserFriendlyAssemblyGroupingKeys.ThirdParty;
                })
                .OrderBy(g => g.Key == UserFriendlyAssemblyGroupingKeys.UserCode ? 1 : g.Key == UserFriendlyAssemblyGroupingKeys.Unity ? 2 : 3)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        internal static void PersistAssemblyPatchInfoEntries()
        {
            EditorPrefs.SetString(
                TransformChangesDebuggerEditorPrefs.AssemblyInfoToPatchEntries,
                JsonUtility.ToJson(
                    new AssemblyPatchInfoWrapper(AssemblyPatchInfoEntries)
                )
            );
        }

        internal static void UpdateDataOnFrameStart(int currentFrame)
        {
            if(currentFrame != _updateGuiVisibleDataLastRunInFrame)
                UpdateGuiVisibleData();
        }

        internal static void HandleSelectionChanged(TransformChangesDebuggerNodeBase selectedNode)
        {
            CurrentlySelectedNode = selectedNode;

            if (IsHistoryPreviewEnabled)
            {
                TryPreviewChange();
            }
        }

        private static void TryPreviewChange()
        {
            if (Selection.objects.Length == 0) return;

            var changedTransformNode = Selection.objects[0] as TransformChangeNode;
            if (changedTransformNode)
            {
                if (!EditorApplication.isPaused) EditorApplication.isPaused = true;
                Selection.activeGameObject = changedTransformNode.ModifiedObject.gameObject;
                changedTransformNode.SetChangeToModifiedObject();
            }
        }

        internal static void UpdateGuiVisibleData()
        {
            _updateGuiVisibleDataLastRunInFrame = Time.frameCount;
            AllTrackedObjects = TransformChangesTracker.GetTrackedObjects();
            if (SelectedTrackedObject)
            {
                SelectedFrameIndexToChangesForSelectedObjectMap.Clear();
                for (int i = 0; i < ShowNFrames; i++)
                {
                    var frameAdjustment = SkipNFramesFromLastCaptured - i;

                    var frameIndex = TransformChangesTracker.GetNewestFrameNumberWithTrackedChanges(-frameAdjustment);
                    if(SelectedFrame == 0) _selectedFrame = frameIndex; //initialize selected frame, make sure not to use property as that'll cause recursive loop
                    if (!SelectedFrameIndexToChangesForSelectedObjectMap.ContainsKey(frameIndex))
                    {
                        var frameChanges = TransformChangesTracker.GetFrameChangesForTrackedObject(frameIndex, SelectedTrackedObject);
                        SelectedFrameIndexToChangesForSelectedObjectMap.Add(frameIndex, frameChanges);
                    }
                }
                
                TransformModifiersForSelectedObjectAndFrameGroupedByCallingObjectName = TransformChangesTracker.CreateModifiers(SelectedFrameIndexToChangesForSelectedObjectMap
                    .SelectMany(kv => kv.Value))
                    .GroupBy(c => c.CallingObject ? c.CallingObject.name : "Static / Unknown")
                    .ToDictionary(g => g.Key, g => g.ToList());
            }
        }
    }
    
    /// <summary>
    /// Event arguments that contain user selected options on what to display on main screen
    /// </summary>
    public class GraphDrawingDependedSettingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Currently selected object <see cref="TransformChangesDebuggerGuiManager.SelectedTrackedObject"/>
        /// </summary>
        public TrackTransformChanges SelectedObject { get; }
        public Dictionary<int, List<TransformChange>> FrameIndexToChangesForSelectedObjectMap{ get; }
        
        /// <summary>
        /// Frames to show count <see cref="TransformChangesDebuggerGuiManager.ShowNFrames"/>
        /// </summary>
        public int ShowNFrames { get; }
        
        /// <summary>
        /// Are position related changes displayed <see cref="TransformChangesDebuggerGuiManager.ShowChangesForPosition"/>
        /// </summary>
        public bool ShowChangesForPosition { get; set; }
        
        /// <summary>
        /// Are rotation related changes displayed <see cref="TransformChangesDebuggerGuiManager.ShowChangesForRotation"/>
        /// </summary>
        public bool ShowChangesForRotation { get; set; }
        
        /// <summary>
        /// Are scale related changes displayed <see cref="TransformChangesDebuggerGuiManager.ShowChangesForScale"/>
        /// </summary>
        public bool ShowChangesForScale { get; set; }


        public GraphDrawingDependedSettingChangedEventArgs(TrackTransformChanges selectedObject, Dictionary<int, List<TransformChange>> frameIndexToChangesForSelectedObjectMap, 
             int showNFrames, bool showChangesForPosition, bool showChangesForRotation, bool showChangesForScale)
        {
            SelectedObject = selectedObject;
            FrameIndexToChangesForSelectedObjectMap = frameIndexToChangesForSelectedObjectMap;
            ShowNFrames = showNFrames;
            ShowChangesForPosition = showChangesForPosition;
            ShowChangesForRotation = showChangesForRotation;
            ShowChangesForScale = showChangesForScale;
        }
    }

    /// <summary>
    /// Details about assembly available to patch 
    /// </summary>
    [Serializable]
    public class AssemblyPatchInfo
    {
        public string Name;
        public string AssemblyFilePath;
        public int LastPatchedMethodCount;
        public long LastPatchedTimeTaken;
        public bool IsPatchingEnabled;

        public AssemblyPatchInfo(string name, string assemblyFilePath, int lastPatchedMethodCount, long lastPatchedTimeTaken, bool isPatchingEnabled)
        {
            Name = name;
            AssemblyFilePath = assemblyFilePath;
            LastPatchedMethodCount = lastPatchedMethodCount;
            LastPatchedTimeTaken = lastPatchedTimeTaken;
            IsPatchingEnabled = isPatchingEnabled;
        }

        public AssemblyPatchInfo()
        {
        }
    }
    
    internal enum GeneralFlagSetting
    {
        IsTrackingEnabled,
        IsHistoryPreviewEnabled
    }
}