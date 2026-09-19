using System.Collections.Generic;
using System.Linq;
using ImmersiveVRTools.Editor.Common.Utilities;
using ImmersiveVRTools.Runtime.Common.Extensions;
using ImmersiveVRTools.Runtime.Common.Utilities;
using TransformChangesDebugger.API;
using TransformChangesDebugger.Editor;
using TransformChangesDebugger.Editor.GUI;
using TransformChangesDebugger.Editor.XNodeExtension.EditorWindowAddons;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor
{
    internal class ModifiersSidebar : ScrollableBarEditorWindowAddon
    {
        public static new readonly int Width = 300;
        private static readonly int CallingMethodLabelWidth = (int)(Width * 0.75f);
        private static readonly int MaxMethodNameLengthToShow = 30;
        
        private readonly Dictionary<string, bool> _callingObjectNameToIsOpenedMap = new Dictionary<string, bool>();
        private static readonly string HelpText = "This pane will provide details about objects that are changing selected TrackedObject. " +
                                           "\n\nIt groups modifiers by game object and the further lists calling method name.";
        
        private static readonly Color Red = new Color(1f, 0.0f, 0.0f, 0.2f);
        private static readonly Color Blue = new Color(0.0f, 0.0f, 1f, 0.7f);
        private static readonly Color Green = new Color(0.0f, 1f, 0.0f, 0.2f);
        
        private static GUIStyle RedBackground => CreateBackgroundColorStyle("Red", Red);
        private static GUIStyle BlueBackground => CreateBackgroundColorStyle("Blue", Blue);
        private static GUIStyle GreenBackground => CreateBackgroundColorStyle("Green", Green);

        private static Dictionary<Color, GUIStyle> _colorToLabelGuiStyleMap;
        private static Dictionary<Color, GUIStyle> ColorToLabelGuiStyleMap
        {
            get
            {
                if (_colorToLabelGuiStyleMap == null)
                {
                    _colorToLabelGuiStyleMap = new Dictionary<Color, GUIStyle>()
                    {
                        [Red] = RedBackground,
                        [Blue] = BlueBackground,
                        [Green] = GreenBackground
                    };
                }

                return _colorToLabelGuiStyleMap;
            }
        }

        private static GUIStyle CreateBackgroundColorStyle(string colorKey, Color color)
        {
            return CachedGUIStyle.GetOrCreate(
                $"{nameof(ModifiersSidebar)}.{colorKey}Background",
                () => new GUIStyle(GUI.skin.label) { normal = {background = TextureHelper.CreateGuiBackgroundColor(color)}  }
            );
        }
        
        public ModifiersSidebar(NodeEditorWindowAddonAttachment nodeEditorWindowAddonAttachment, int height) : base(nodeEditorWindowAddonAttachment, Width, height)
        {
        }

        protected override void DrawInternal()
        {

            GUILayout.BeginHorizontal();
            GUILayout.Label("Modifiers", TransformChangesDebuggerStyles.SidebarHeader, GUILayout.Width(Width - TransformChangesDebuggerStyles.HelperTooltipWidth));
            TransformChangesDebuggerStyles.AddHelperTooltip(HelpText, new Vector4(0, 7, 0, 0));
            GUILayout.EndHorizontal();

            if (!TransformChangesDebuggerGuiManager.TransformModifiersForSelectedObjectAndFrameGroupedByCallingObjectName.Any())
            {
                GUILayout.Space(20);
                if(!TransformChangesDebuggerGuiManager.SelectedTrackedObject)
                    GUILayout.Label("Select tracked object (left panel) first.", TransformChangesDebuggerStyles.EmphasisedText);
                else                
                    GUILayout.Label("No modifiers for selected object / frame, you can adjust visible frames in top right corner.", TransformChangesDebuggerStyles.EmphasisedText);
                
                GUILayout.Label("\n\n\n" + HelpText, TransformChangesDebuggerStyles.WrapText , GUILayout.Width(Width - 10));
                
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(Width - 85);
            GUILayout.Label("Disable\r\nChanges?");
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(Width - 70);
            TransformChangesDebuggerStyles.AddHelperTooltip("Changes done by modifiers can be turned off ad-hoc (without the need to recompile).\n\n" +
                                                            "Simply select the checkbox and unpause.");
            GUILayout.EndHorizontal();

            
            foreach (var callingObjectNameToTransformModifierKv in TransformChangesDebuggerGuiManager.TransformModifiersForSelectedObjectAndFrameGroupedByCallingObjectName)
            {
                var callingObjectName = callingObjectNameToTransformModifierKv.Key;
                
                if(!_callingObjectNameToIsOpenedMap.ContainsKey(callingObjectName))
                    _callingObjectNameToIsOpenedMap.Add(callingObjectName, true);

                var isOpened = _callingObjectNameToIsOpenedMap[callingObjectName];

                _callingObjectNameToIsOpenedMap[callingObjectName] = EditorGUILayout.BeginFoldoutHeaderGroup(isOpened, callingObjectName);
                foreach (var transformModifier in callingObjectNameToTransformModifierKv.Value)
                {
                    if (isOpened)
                    {
                        var areChangesDisabled = TransformChangesDebuggerManager.ShouldSkipTransformChangeDueToDisabledModifier(transformModifier);
                    
                        GUILayout.BeginHorizontal();
                        var transformModifierCallingObjectType = transformModifier.CallingObject.GetType();
                        var fullMethodName =  $"{transformModifierCallingObjectType.Name}.{TransformChange.SimplifyMethodName(transformModifier.CallingFromMethodName, transformModifierCallingObjectType)}";
                        var wasMethodNameTrimmed = fullMethodName.TryTrimExcess(MaxMethodNameLengthToShow, out var trimmedMethodName);
                        var labelContent = new GUIContent(trimmedMethodName,  wasMethodNameTrimmed ? fullMethodName : string.Empty);

                        var styleForLabel = TransformChangesDebuggerGuiManager.TransformModifierToUseColorMap.TryGetValue(transformModifier, out var foundColor) 
                            ? ColorToLabelGuiStyleMap[foundColor] 
                            : GUI.skin.label;
                        GUILayout.Label(labelContent, styleForLabel, GUILayout.Width(CallingMethodLabelWidth));

                        if (GUILayout.Toggle(areChangesDisabled, "", TransformChangesDebuggerStyles.ModifierToggle))
                            TransformChangesDebuggerManager.SkipTransformChangesFor(transformModifier);
                        else
                            TransformChangesDebuggerManager.RemoveSkipTransformChangesFor(transformModifier);

                        var additionalOptionsButtonContext = CachedGUIContent.GetOrCreate(
                            $"{nameof(ModifiersSidebar)}.AdditionalOptions",
                            () => EditorGUIUtility.TrIconContent("_Menu", "Additional Options")
                        );             
                        var rect = GUILayoutUtility.GetRect(additionalOptionsButtonContext, EditorStyles.toolbarButton);
                        if (GUILayout.Button(additionalOptionsButtonContext, EditorStyles.toolbarButton))
                        {
                            var genericMenu = new GenericMenu();
                            genericMenu.AddItem(CachedGUIContent.Text("Disable Changes"), areChangesDisabled, () =>
                            {
                                //toggle, if was disabled - enable and other way round
                                if (!areChangesDisabled)
                                    TransformChangesDebuggerManager.SkipTransformChangesFor(transformModifier);
                                else 
                                    TransformChangesDebuggerManager.RemoveSkipTransformChangesFor(transformModifier);
                            });

                            genericMenu.AddSeparator("");
                            CreateMarkChangesWithColorMenuItem(genericMenu, transformModifier, "red", Red);
                            CreateMarkChangesWithColorMenuItem(genericMenu, transformModifier, "green", Green);
                            CreateMarkChangesWithColorMenuItem(genericMenu, transformModifier, "blue", Blue);
                            genericMenu.AddItem(CachedGUIContent.Text($"Mark changes - none"), 
                                TransformChangesDebuggerGuiManager.TransformModifierToUseColorMap.TryGetValue(transformModifier, out var col) ? false : true, 
                                () =>
                                {
                                    TransformChangesDebuggerGuiManager.StopUsingColorForChangesDoneViaModifiers(transformModifier);
                                }
                            );
                            
                            genericMenu.AddSeparator("");
                            genericMenu.AddItem(CachedGUIContent.Text("Show next"), false, () => TransformChangesDebuggerGuiManager.NavigateToNextNode(transformModifier));
                            genericMenu.AddItem(CachedGUIContent.Text("Show previous"), false, () => TransformChangesDebuggerGuiManager.NavigateToPreviousNode(transformModifier));
                            
                            genericMenu.DropDown(rect);
                        }
                    
                        GUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private static void CreateMarkChangesWithColorMenuItem(GenericMenu genericMenu, TransformModifier transformModifier, string colorName, Color color)
        {
            genericMenu.AddItem(CachedGUIContent.Text($"Mark changes - {colorName}"), 
                IsModifierUsedWithColor(transformModifier, color), 
                () => {
                    TransformChangesDebuggerGuiManager.UseColorForChangesDoneViaModifiers(transformModifier, color);
                }
            );
        }

        private static bool IsModifierUsedWithColor(TransformModifier transformModifier, Color color)
        {
            return TransformChangesDebuggerGuiManager.TransformModifierToUseColorMap.TryGetValue(transformModifier, out var col) ? col == color : false;
        }
    }
}