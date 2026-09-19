using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ImmersiveVRTools.Editor.Common.Utilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ImmersiveVRTools.Runtime.Common.ScriptableObject;
using ImmersiveVRTools.Runtime.Common.Utilities;
using Debug = UnityEngine.Debug;

namespace ImmersiveVRTools.Editor.Common.Editor
{
    public abstract class NestedScriptableObjectEditorBase<TNestedScriptableObject> : UnityEditor.Editor
        where TNestedScriptableObject: ScriptableObject, IOrderableNestedScriptableObject
    {
        protected virtual List<string> MovePropertyNamesToTop { get; } = new List<string>();
        protected abstract string NestedPropertyName { get; }
        protected abstract string NestedPropertyLabel { get; }

        protected abstract List<AddNestedScriptableObjectParams> AddNestedScriptableObjectParams { get; }
        protected abstract string NameBoundProperty { get; } //WARN: for all entries this has to be same, would have used interface although it has to be serializable field name and not property name
        
        
        private static float SpacingBetweenElements = EditorGUIUtility.singleLineHeight;
        private static GUIStyle _headerLabelStyle;

        private ReorderableList _nestedEntries;
        private SerializedProperty _mappingsProperty;
        
        private static float NameChangeAssetReloadDebounce = 1f;

        private Stopwatch _nameChangeSaveDebounceStopwatch = new Stopwatch();
        
        public void OnEnable()
        {
            EnsureNameBoundPropertyExists();
            EnsureAddObjectsHaveCorrectTypes();
            
            _mappingsProperty = serializedObject.FindProperty(NestedPropertyName);

            _nestedEntries = new ReorderableList(
                    serializedObject, 
                    _mappingsProperty, 
                    draggable: true,
                    displayHeader: true,
                    displayAddButton: true,
                    displayRemoveButton: true);

            _nestedEntries.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, NestedPropertyLabel);
            };

            _nestedEntries.onRemoveCallback = (ReorderableList l) => {
                var element = l.serializedProperty.GetArrayElementAtIndex(l.index); 
                var obj = element.objectReferenceValue;

                AssetDatabase.RemoveObjectFromAsset(obj);

                DestroyImmediate(obj, true);
                l.serializedProperty.DeleteArrayElementAtIndex(l.index);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            };

            _nestedEntries.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                var element = _mappingsProperty.GetArrayElementAtIndex(index);

                rect.y += 2;
                rect.width -= 10;
                rect.height = EditorGUIUtility.singleLineHeight;

                if (element.objectReferenceValue == null) {
                    return;
                }
                var label = $"{element.objectReferenceValue.name} ({element.objectReferenceValue.GetType().Name})";
                EditorGUI.LabelField(rect, label, _headerLabelStyle);

                // Convert this element's data to a SerializedObject so we can iterate
                // through each SerializedProperty and render a PropertyField.
                var nestedObject = new SerializedObject(element.objectReferenceValue);

                // TODO: for sorting to work we need to change that iteration / rendering, is copying properties going to cause perf issues?
                var currentPropertyIterator = nestedObject.GetIterator();
                var properties = new List<SerializedProperty>();
                while (currentPropertyIterator.NextVisible(true))
                {
                    if (currentPropertyIterator.name == "m_Script") {
                        continue;
                    }
                    
                    var propertyCopy = currentPropertyIterator.Copy();
                    properties.Add(propertyCopy);
                }

                var orderIndex = 1000; //start with high number so the ones with actual index will float to top as needed
                properties = properties.OrderBy(p =>
                {
                    var index1 = MovePropertyNamesToTop.IndexOf(p.name);
                    if (index1 != -1) return index1;

                    return orderIndex++;
                }).ToList();
                
                rect.y += EditorGUIUtility.singleLineHeight;
                foreach (var prop in properties)
                {
                    try
                    {
                        if (prop.name == NameBoundProperty)
                        {
                            if (!string.Equals(prop.stringValue, prop.serializedObject.targetObject.name))
                            {
                                prop.serializedObject.targetObject.name = prop.stringValue;
                            
                                _nameChangeSaveDebounceStopwatch.Restart();
                            }
                        }

                        if (IsArraySizeProperty(prop))
                        {
                            //TODO: is skipping size ok in different Unity versions? eg will side panel be built for that at all?
                            //skip drawing size for arrays
                        }
                        else
                        {
                            RenderNestedProperty(prop, nestedObject, ref rect);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e);
                        //TODO: handle error - this is annoying but when trying to remove array element, a 'missing element' / out of bounds is thrown, then SetDirty never happens and item is not saved
                    }
                }

                nestedObject.ApplyModifiedProperties();

                // Mark edits for saving
                if (GUI.changed) {
                    EditorUtility.SetDirty(target);
                }
            };

            _nestedEntries.elementHeightCallback = (int index) => {
                float baseProp = EditorGUI.GetPropertyHeight(
                    _nestedEntries.serializedProperty.GetArrayElementAtIndex(index), true);

                float additionalProps = 0;
                SerializedProperty element = _mappingsProperty.GetArrayElementAtIndex(index);
                if (element.objectReferenceValue != null) {
                    SerializedObject serializedObject = new SerializedObject(element.objectReferenceValue);
                    SerializedProperty prop = serializedObject.GetIterator();
                    while (prop.NextVisible(true)) {
                        // XXX: This logic stays in sync with loop in drawElementCallback.
                        if (prop.name == "m_Script" || IsArraySizeProperty(prop)) {
                            continue;
                        }
                        additionalProps += EditorGUI.GetPropertyHeight(prop);
                    }
                }
                
                return baseProp + SpacingBetweenElements + additionalProps;
            };

            _nestedEntries.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) => {
                var menu = new GenericMenu();
                foreach (var param in AddNestedScriptableObjectParams)
                {
                    menu.AddItem(
                        new GUIContent(param.AddButtonText),
                        false,
                        HandleAddNew,
                        param
                    );
                }

                menu.ShowAsContext();
            };
            
            _nestedEntries.onReorderCallback = (ReorderableList list) => { EnsureOrdered(list); };
            
            EnsureOrdered(_nestedEntries);
        }
        
        private static readonly List<int> _ButtonSizes = new List<int>() { 70 };
        protected virtual void RenderNestedProperty(SerializedProperty property, SerializedObject nestedObject, ref Rect editorRect)
        {
            if (property.propertyType == SerializedPropertyType.Generic && property.isArray)
            {
                property.isExpanded = false; //WARN: this is to prevent lists from being expanded / collapsed as that messes up editor view
                
                int totalWidthTaken;
                var buttonRects = GUIRectHelper.GetInlineButtonRects(editorRect, out totalWidthTaken, _ButtonSizes, 5);
                
                GUI.enabled = false;
                var propRect = new Rect(editorRect);
                propRect.xMax -= totalWidthTaken;
                EditorGUI.PropertyField(propRect, property);
                GUI.enabled = true;

                if (GUI.Button(buttonRects[0], "New Entry"))
                {
                    property.arraySize++;
                    property.GetArrayElementAtIndex(property.arraySize - 1).objectReferenceValue = null;
                    nestedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.PropertyField(editorRect, property);
            }
            
            editorRect.y += EditorGUIUtility.singleLineHeight;
        }

        private static bool IsArraySizeProperty(SerializedProperty prop)
        {
            return prop.type == "ArraySize" && prop.name == "size";
        }

        private static void EnsureOrdered(ReorderableList list)
        {
            var index = 0;
            foreach (var listElement in list.serializedProperty)
            {
                ((TNestedScriptableObject) ((SerializedProperty) listElement).objectReferenceValue).Order = index++;
            }
        }


        private void HandleAddNew(object dataObj)
        {
            var data = (AddNestedScriptableObjectParams) dataObj;
            
            // Make room in list
            var index = _nestedEntries.serializedProperty.arraySize;
            _nestedEntries.serializedProperty.arraySize++;
            _nestedEntries.index = index;
            var element = _nestedEntries.serializedProperty.GetArrayElementAtIndex(index);

            var nestedScriptableObject = ScriptableObject.CreateInstance(data.AddObjectType);
            nestedScriptableObject.name = $"New{data.AddObjectType.Name}"; ;

            if (!EditorUtility.IsPersistent(target))
            {
                throw new UnityException("Trying to add object to non-persistent target.");
            }
            
            AssetDatabase.AddObjectToAsset(nestedScriptableObject, target);
            AssetDatabase.SaveAssets();
            element.objectReferenceValue = nestedScriptableObject;
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {           
            InitStyles();
            
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, NestedPropertyName);

            _nestedEntries.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
            
            ReimportAssetToReflectNameChangesWithDebounce();
        }

        private static void InitStyles()
        {
            if (_headerLabelStyle == null || _headerLabelStyle.normal.background == null)
            {
                _headerLabelStyle = new GUIStyle(GUI.skin.label);
                _headerLabelStyle.normal = new GUIStyleState()
                    {background = TextureHelper.CreateGuiBackgroundColor(new Color(0.0f, 1f, 0.0f, 0.2f))};
                _headerLabelStyle.normal.textColor = Color.white;
                _headerLabelStyle.fontStyle = FontStyle.Bold;
            }
        }

        private void ReimportAssetToReflectNameChangesWithDebounce()
        {
            if (_nameChangeSaveDebounceStopwatch.Elapsed.Seconds > NameChangeAssetReloadDebounce)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                _nameChangeSaveDebounceStopwatch.Reset();
            }
        }
        
        private void EnsureAddObjectsHaveCorrectTypes()
        {
            foreach (var addNestedScriptableObjectParams in AddNestedScriptableObjectParams)
            {
                if (!typeof(TNestedScriptableObject).IsAssignableFrom(addNestedScriptableObjectParams.AddObjectType))
                {
                    throw new Exception($"Add nested scriptable object is not set correctly. {addNestedScriptableObjectParams.AddObjectType.Name} does not inherit from {typeof(TNestedScriptableObject).Name}");

                }
            }
        }

        private void EnsureNameBoundPropertyExists()
        {
            foreach (var addNestedScriptableObjectParams in AddNestedScriptableObjectParams)
            {
                var nameBoundField = addNestedScriptableObjectParams.AddObjectType.GetField(NameBoundProperty,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (nameBoundField == null)
                {
                    throw new Exception(
                        $"Invalid {nameof(NameBoundProperty)}:{NameBoundProperty} setup - field not existing on type: {addNestedScriptableObjectParams.AddObjectType.Name}");
                }
            }
        }
    }

    public class AddNestedScriptableObjectParams
    {
        public string AddButtonText { get; }
        public Type AddObjectType { get; }

        public AddNestedScriptableObjectParams(string addButtonText, Type addObjectType)
        {
            AddButtonText = addButtonText;
            AddObjectType = addObjectType;
        }
    }
    
    public class NestedScriptableObjectEditorCustomPropertyRenderMethodResult
    {
        public bool ShouldUseDefaultRenderer { get; }
        public int CustomRenderingHeight { get; }

        public NestedScriptableObjectEditorCustomPropertyRenderMethodResult(bool shouldUseDefaultRenderer, int customRenderingHeight)
        {
            ShouldUseDefaultRenderer = shouldUseDefaultRenderer;
            CustomRenderingHeight = customRenderingHeight;
        }
    }
}