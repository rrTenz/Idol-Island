using System.Collections.Generic;
using System.Linq;
using ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs;
using TransformChangesDebugger.Editor;
using TransformChangesDebugger.Editor.GUI;
using TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons;
using TransformChangesDebugger.Runtime;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor
{
    internal class PatchedAssembliesBar : ScrollableBarEditorWindowAddon
    {
        private int AssemblyNameLabelWidth => (int) (Width * 0.4f);
        private int AssemblyPatchInfoLabelWidth => (int) (Width * 0.4f);
        private readonly int HeaderLabelWidth = 130;

        private EditorPrefsAutoPersistingStringToBoolDictionaryMap _assemblyUserFriendlyGroupNameToIsOpenedMap;
        private EditorPrefsAutoPersistingStringToBoolDictionaryMap AssemblyUserFriendlyGroupNameToIsOpenedMap
        {
            get
            {
                if (_assemblyUserFriendlyGroupNameToIsOpenedMap == null)
                {
                    _assemblyUserFriendlyGroupNameToIsOpenedMap = new EditorPrefsAutoPersistingStringToBoolDictionaryMap(TransformChangesDebuggerEditorPrefs.AssemblyUserFriendlyGroupNameToIsOpenedMap, 
                        new Dictionary<string, bool>()
                        {
                            [UserFriendlyAssemblyGroupingKeys.UserCode] = true,
                            [UserFriendlyAssemblyGroupingKeys.ThirdParty] = false,
                            [UserFriendlyAssemblyGroupingKeys.Unity] = false,
                        }
                    );
                }

                return _assemblyUserFriendlyGroupNameToIsOpenedMap;
            }
        }

        private static readonly string HelpText = "You can use this window to select which assemblies the tool show redirect calls from." +
                                                  "\n\nMost of the time you'll want all 'User Code' to be enabled - this will be set by default." +
                                                  "\n\nYou can also expand selection to other assemblies, for example if you suspect 3rd party code or Unity code is making changes to your transforms.";

        private GUIStyle _foldoutHeaderStyle;
        private GUIStyle FoldoutHeaderStyle
        {
            get
            {
                if (_foldoutHeaderStyle == null)
                {
                    _foldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);
                }

                return _foldoutHeaderStyle;
            }
        }

        private GUIStyle _patchingTimeEstimateLabelStyle;
        private GUIStyle PatchingTimeEstimateLabelStyle
        {
            get
            {
                if (_patchingTimeEstimateLabelStyle == null)
                {
                    _patchingTimeEstimateLabelStyle = new GUIStyle(TransformChangesDebuggerStyles.WrapText) {
                        margin = new RectOffset(0, 0, 0, 0)
                    };
                }

                return _patchingTimeEstimateLabelStyle;
            }
        }

        public PatchedAssembliesBar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int width, int height) : base(nodeEditorWindowAddonAttachment, width, height)
        {
        }

        protected override void DrawInternal()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Assemblies", TransformChangesDebuggerStyles.SidebarHeader, GUILayout.ExpandWidth(true));
            
            TransformChangesDebuggerStyles.AddHelperTooltip(HelpText, new Vector4(0, 7, 0, 0));
            GUILayout.EndHorizontal();
            
            if (!TransformChangesDebuggerGuiManager.UserFriendlyGroupNameToAssemblyPatchInfoEntries.Any())
            {
                GUILayout.Label(HelpText, GUILayout.ExpandWidth(true));
                return;
            }
            
            GUILayout.Label("If your changes are still not captured make sure to select assemblies that could be making the change. By default tool will only track user code.", TransformChangesDebuggerStyles.EmphasisedText, GUILayout.ExpandWidth(true));
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(AssemblyNameLabelWidth + 5);
            GUILayout.Label($"{(!Application.isPlaying ? "(Patching will happen when entering playmode)" : "\n")}" +
                            $"\n({(!Application.isPlaying ? "Will take approximately" : "Total time taken")} {TransformChangesDebuggerGuiManager.AssemblyPatchInfoEntries.Where(p => p.IsPatchingEnabled).Sum(e => e.LastPatchedTimeTaken)}ms, " +
                            $"{(!Application.isPlaying ? "to patch" : "patched")} {TransformChangesDebuggerGuiManager.AssemblyPatchInfoEntries.Where(p => p.IsPatchingEnabled).Sum(e => e.LastPatchedMethodCount)} methods)",
                PatchingTimeEstimateLabelStyle, GUILayout.Width(AssemblyPatchInfoLabelWidth - 30)
            );
            GUILayout.Label("Capture changes\nfrom assembly?", TransformChangesDebuggerStyles.WrapText, GUILayout.Width(100));
            TransformChangesDebuggerStyles.AddHelperTooltip("Changes originating from selected assemblies will be captured.", new Vector4(0, 11, 0, 0));
            GUILayout.EndHorizontal();
            
            foreach (var assemblyPatchInfoGroup in TransformChangesDebuggerGuiManager.UserFriendlyGroupNameToAssemblyPatchInfoEntries)
            {
                var isFoldoutOpened = AssemblyUserFriendlyGroupNameToIsOpenedMap[assemblyPatchInfoGroup.Key];

                GUILayout.BeginHorizontal();
                //HACK: foldout rects are precalculated to handle clicks toggle button clicks in foldout area, we need to adjust some values so toggle is triggered instead of fold/expand 
                var foldoutRect = GUILayoutUtility.GetRect(new GUIContent(assemblyPatchInfoGroup.Key), FoldoutHeaderStyle);
                foldoutRect.width = Width;
                var eventTypeBeforeFoldout = Event.current.type;
                //rect calculation needs to happen before EditorGUI.BeginFoldoutHeaderGroup as that'll change positions
                var selectAllTogglePosition = new Rect(foldoutRect.x + AssemblyNameLabelWidth + AssemblyPatchInfoLabelWidth + 9, foldoutRect.y, 16, 16);
                
                AssemblyUserFriendlyGroupNameToIsOpenedMap[assemblyPatchInfoGroup.Key] = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, isFoldoutOpened, assemblyPatchInfoGroup.Key, FoldoutHeaderStyle);
                if(AssemblyUserFriendlyGroupNameToIsOpenedMap[assemblyPatchInfoGroup.Key] != isFoldoutOpened) AssemblyUserFriendlyGroupNameToIsOpenedMap.PersistChanges();

                var allEnabledForGroup = assemblyPatchInfoGroup.Value.All(a => a.IsPatchingEnabled);
                HandleToggleAllClickInFoldoutArea(isFoldoutOpened, assemblyPatchInfoGroup, eventTypeBeforeFoldout, selectAllTogglePosition, allEnabledForGroup);
                
                GUI.Toggle(selectAllTogglePosition, allEnabledForGroup, "", TransformChangesDebuggerStyles.AssyToPatchToggle);
                    
                GUILayout.EndHorizontal();
                
                foreach (var assemblyPatchInfoEntry in assemblyPatchInfoGroup.Value)
                {
                    if(isFoldoutOpened) {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"{assemblyPatchInfoEntry.Name}", GUILayout.Width(AssemblyNameLabelWidth));
                        var patchedInfo = ResolvePatchedInfoMessage(assemblyPatchInfoEntry);
                        GUILayout.Label(patchedInfo, GUILayout.Width(AssemblyPatchInfoLabelWidth));
                        
                        var isAssyPatchingEnabled = assemblyPatchInfoEntry.IsPatchingEnabled;
                        if (GUILayout.Toggle(isAssyPatchingEnabled, isAssyPatchingEnabled ? "" : "", TransformChangesDebuggerStyles.AssyToPatchToggle))
                        {
                            var wasChanged = !isAssyPatchingEnabled;
                            assemblyPatchInfoEntry.IsPatchingEnabled = true;
                            if (wasChanged)
                            {
                                HandleAsseblyPatchingChanged();
                            }
                        }
                        else
                        {
                            var wasChanged = isAssyPatchingEnabled;
                            assemblyPatchInfoEntry.IsPatchingEnabled = false;
                            if (wasChanged)
                            {
                                TransformChangesDebuggerGuiManager.PersistAssemblyPatchInfoEntries();
                            }
                        }
                        
                        GUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUILayout.Space(1);
        }

        private static void HandleAsseblyPatchingChanged()
        {
            //if outside of playmode just persist, that'll be loaded up on play and applied. Otherwise apply immediately
            if (Application.isPlaying)
            {
                TransformChangesDebuggerGuiManager.PatchSettersForSelectedAssemblies();
            }

            else
                TransformChangesDebuggerGuiManager.PersistAssemblyPatchInfoEntries();
        }

        //This is needed due to the way foldouts work, they don't generally allow other clicks in their area, when that happens we need to manually revert foldout changes made and trigger toggle
        private void HandleToggleAllClickInFoldoutArea(bool isOpened, KeyValuePair<string, List<AssemblyPatchInfo>> assemblyPatchInfoGroup,
            EventType eventTypeBeforeFoldout, Rect selectAllTogglePosition, bool allEnabledForGroup)
        {
            if (isOpened != AssemblyUserFriendlyGroupNameToIsOpenedMap[assemblyPatchInfoGroup.Key])
            {
                if (eventTypeBeforeFoldout == EventType.MouseUp)
                {
                    if (selectAllTogglePosition.Contains(Event.current.mousePosition))
                    {
                        if (allEnabledForGroup) //previously all enabled - disable
                        {
                            assemblyPatchInfoGroup.Value.ForEach(a => a.IsPatchingEnabled = false);
                        }
                        else //previously not all enabled - enable
                        {
                            assemblyPatchInfoGroup.Value.ForEach(a => a.IsPatchingEnabled = true);
                            HandleAsseblyPatchingChanged();
                        }

                        AssemblyUserFriendlyGroupNameToIsOpenedMap[assemblyPatchInfoGroup.Key] = isOpened; //revert foldout isOpened back to original (as it'll be closed due to click in foldout area)
                    }
                }
            }
        }

        private static readonly HashSet<string> KnownAssembliesTakingLongToPatch = new HashSet<string>()
        {
            "UnityEngine.CoreModule.dll",    
            "UnityEditor.CoreModule.dll"
        };
        
        private static string ResolvePatchedInfoMessage(AssemblyPatchInfo assemblyPatchInfoEntry)
        {
            string patchedInfo;
            if (assemblyPatchInfoEntry.IsPatchingEnabled)
            {
                if (Application.isPlaying)
                {
                    if (assemblyPatchInfoEntry.LastPatchedMethodCount == 0)
                        patchedInfo = "(no methods found to patch)";
                    else
                        patchedInfo = $"(patched {assemblyPatchInfoEntry.LastPatchedMethodCount} methods in {assemblyPatchInfoEntry.LastPatchedTimeTaken}ms)";
                }
                else
                {
                    if (assemblyPatchInfoEntry.LastPatchedMethodCount > 0)
                        patchedInfo = $"(previously patched {assemblyPatchInfoEntry.LastPatchedMethodCount} methods in {assemblyPatchInfoEntry.LastPatchedTimeTaken}ms)";
                    else
                        patchedInfo = "(enter playmode to patch)";
                }
            }
            else
            {
                patchedInfo = "(not selected)";
            }

            if (assemblyPatchInfoEntry.LastPatchedMethodCount == 0 && KnownAssembliesTakingLongToPatch.Contains(assemblyPatchInfoEntry.Name))
            {
                patchedInfo += "\n[WARN: first time patching will take longer time to build cache]";
            }

            return patchedInfo;
        }
    }
}