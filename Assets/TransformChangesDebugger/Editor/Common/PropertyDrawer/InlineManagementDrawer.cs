using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ImmersiveVRTools.Editor.Common.Utilities;
using UnityEditor;
using UnityEngine;
using ImmersiveVRTools.Runtime.Common.PropertyDrawer;
using ImmersiveVRTools.Runtime.Common.Utilities;

namespace ImmersiveVRTools.Editor.Common.PropertyDrawer
{
    [CustomPropertyDrawer(typeof(InlineManagementAttribute), true)]
    public class InlineManagementDrawer : UnityEditor.PropertyDrawer
    {
        private static InlineManagementAttribute _nullReferenceOptionsManagementAttribute = new InlineManagementAttribute()
        {
            AfterCreationNewRunMethodName = string.Empty
        };
    
        private static GUIStyle _ButtonStyle;
        private static GUIStyle ButtonStyle => _ButtonStyle ?? (_ButtonStyle =   new GUIStyle(GUI.skin.button));
        
        private static Dictionary<SerializedProperty, int> SerializedPropertyToRemoveAtIndexDeferredDeleteMap = new Dictionary<SerializedProperty, int>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RenderManagementButtonInline(ref position, property, property,
                (pos, prop, includeLabel) =>
                {
                    if (!includeLabel)
                    {
                        EditorGUI.PropertyField(position, prop);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, prop, label);
                    }
                }, fieldInfo
            );

            DeferredRemoveElementsSpecifiedByUser();
        }

        //Those elements need to be removed after OnGUI, otherwise Unity will log errors
        private static void DeferredRemoveElementsSpecifiedByUser()
        {
            foreach (var kv in SerializedPropertyToRemoveAtIndexDeferredDeleteMap)
            {
                kv.Key.DeleteArrayElementAtIndex(kv.Value);
            }

            SerializedPropertyToRemoveAtIndexDeferredDeleteMap.Clear();
        }

        private static int EditButtonSize = 35;
        private static readonly List<int> _ButtonSizesEdit = new List<int>() { EditButtonSize };
        private static readonly List<int> _ButtonSizesEditAndRemove = new List<int>() { EditButtonSize, 15 };
        public static void RenderManagementButtonInline(ref Rect position, SerializedProperty variable, SerializedProperty parentProperty, 
            Action<Rect, SerializedProperty, bool> renderPropertyFieldFn, FieldInfo propertyFieldInfo, string addNewText = "New")
        {
            ScriptableObject createdObject = null;
            int totalWidthTaken;
            var buttonRects =  GUIRectHelper.GetInlineButtonRects(position, out totalWidthTaken, _ButtonSizesEditAndRemove, 1);
            position.xMax -= totalWidthTaken;

            var inlineManagementAttribute = propertyFieldInfo.GetCustomAttribute<InlineManagementAttribute>() ?? _nullReferenceOptionsManagementAttribute;
            if (!variable.objectReferenceValue)
            {
                if (inlineManagementAttribute.IsCreatingNewDisabled)
                    GUI.enabled = false;
                if (GUI.Button(buttonRects[0], addNewText))
                {
                    var createObjectType = GetType(variable);

                    if (createObjectType.IsAbstract)
                    {
                        var allDerivedTypes = ReflectionHelper.GetAllInstantiableTypesDerivedFrom(createObjectType);
                        var menu = new GenericMenu();
                        foreach (var derivedType in allDerivedTypes)
                        {
                            menu.AddItem(
                                new GUIContent(derivedType.Name),
                                false,
                                (userData) =>
                                {
                                    var created = CreateNew(derivedType);
                                    variable.objectReferenceValue = created;
                                    HandleAfterChange(parentProperty, propertyFieldInfo, created, inlineManagementAttribute);
                                },
                                null
                            );
                        }

                        menu.ShowAsContext();
                    }
                    else
                    {
                        createdObject = CreateNew(createObjectType);
                        variable.objectReferenceValue = createdObject;
                    }
                }
                if (inlineManagementAttribute.IsCreatingNewDisabled)
                    GUI.enabled = true;
            }
            else
            {
                if (GUI.Button(buttonRects[0], "Edit"))
                {
                    DedicatedInspectorWindow.InspectTarget(variable.objectReferenceValue, "Inline Management", new Vector2(400, 300));
                }
            }

            if (GUI.Button(buttonRects[1], "-"))
            {
                if (IsArrayElement(variable))
                {
                    var arrayProp = parentProperty.serializedObject.FindProperty(propertyFieldInfo.Name);
                    var clickedIndex = FindObjectArrayIndex(variable, arrayProp);
                    if (clickedIndex == -1) 
                        throw new Exception("Unable to find clicked element!");
                
                    SerializedPropertyToRemoveAtIndexDeferredDeleteMap.Add(arrayProp, clickedIndex);
                }
                else
                {
                    variable.objectReferenceValue = null;
                }

            }
            
            renderPropertyFieldFn(position, variable, !IsArrayElement(variable));

            HandleAfterChange(parentProperty, propertyFieldInfo, createdObject, inlineManagementAttribute);
        }

        private static bool IsArrayElement(SerializedProperty variable)
        {
            return variable.propertyPath.Split('.').Last().Contains("data["); //not ideal, but can't find better way to tell from data if element is array element
        }

        private static int FindObjectArrayIndex(SerializedProperty variable, SerializedProperty arrayProp)
        {
            for (var i = 0; i < arrayProp.arraySize; ++i)
            {
                var objectReference = arrayProp.GetArrayElementAtIndex(i);
                if (variable.propertyPath != null && objectReference != null &&
                    variable.propertyPath == objectReference.propertyPath)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void HandleAfterChange(SerializedProperty parentProperty, FieldInfo propertyFieldInfo, ScriptableObject createdObject, InlineManagementAttribute addNewInlineAttribute)
        {
            if (EditorGUI.EndChangeCheck())
            {
                parentProperty.serializedObject.ApplyModifiedProperties();
                if (createdObject)
                {
                    TryExecuteMethodAfterCreatingNewInline(parentProperty, createdObject,
                        addNewInlineAttribute.AfterCreationNewRunMethodName, propertyFieldInfo.DeclaringType
                    );
                }
            }
        }

        private static bool TryExecuteMethodAfterCreatingNewInline(SerializedProperty property, ScriptableObject createdNewObject, string afterCreationNewRunMethodName, Type declaringType)
        {
            if (createdNewObject && !string.IsNullOrEmpty(afterCreationNewRunMethodName))
            {
                var runMethodAfterCreation = declaringType.GetMethod(afterCreationNewRunMethodName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (runMethodAfterCreation == null)
                {
                    Debug.LogWarning($"Unable to find method '{afterCreationNewRunMethodName}' on type '{declaringType.Name}'");
                }
                else if (runMethodAfterCreation.GetParameters().Length != 0)
                {
                    Debug.LogWarning(
                        $"Method '{afterCreationNewRunMethodName}' found on type '{declaringType.Name}' but it has more than 0 parameters.");
                    runMethodAfterCreation = null;
                }
                else
                {
                    runMethodAfterCreation.Invoke(property.serializedObject.targetObject, new object [0]);
                    return true;
                }
            }

            return false;
        }


        private static string DefaultDest = "Assets";
        private static string LatestRelativePathEditorPrefKey => $"{Application.dataPath.GetHashCode()}_{nameof(InlineManagementDrawer)}_LatestRelativePath";

        private static ScriptableObject CreateNew(Type createObjectType)
        {
            var latestRelativePath = EditorPrefs.GetString(LatestRelativePathEditorPrefKey);
        
            var obj = ScriptableObject.CreateInstance(createObjectType);

            var dest = DefaultDest;
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
                AssetDatabase.Refresh();
            }

            dest = EditorUtility.SaveFilePanel("Save object as", string.IsNullOrEmpty(latestRelativePath) ? dest : latestRelativePath, "New " + createObjectType.Name, "asset");

            if (!string.IsNullOrEmpty(dest) && PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), dest, out dest))
            {
                EditorPrefs.SetString(LatestRelativePathEditorPrefKey, PathUtilities.RemoveLatestPathPart(dest));
                AssetDatabase.CreateAsset(obj, dest);
                AssetDatabase.Refresh();
                return obj;
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
                return null;
            }
        }
    
        private static System.Type GetType(SerializedProperty property)
        {
            var parts = property.propertyPath.Split('.');
 
            var currentType = property.serializedObject.targetObject.GetType();
 
            for (int i = 0; i < parts.Length; i++)
            {
                currentType = currentType.GetField(parts[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance).FieldType;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(currentType) && currentType.IsGenericType)
                {
                    var genericArgs = currentType.GetGenericArguments();
                    if (genericArgs.Length != 1)
                    {
                        throw new Exception($"Unable to create object for {currentType}");
                    }

                    return genericArgs[0];
                }
            }
 
            return currentType;
        }
    }
}