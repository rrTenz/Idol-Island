using System.Reflection;
using UnityEditor;
using UnityEngine;
using ImmersiveVRTools.Runtime.Common.PropertyDrawer;

namespace ImmersiveVRTools.Editor.Common.PropertyDrawer
{
    [CustomPropertyDrawer(typeof(Reference), true)]
    public class ReferenceDrawer : UnityEditor.PropertyDrawer
    {
        private static ReferenceOptionsAttribute nullReferenceOptionsAttribute = new ReferenceOptionsAttribute()
        {
            ForceVariableOnly = false
        };
    
        private readonly string[] _PopupOption = { "Use Constant", "Use Variable" };
        private readonly string[] _ForcedVariablePopupOption = { "Use Variable" };

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle _PopupStyle;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var referenceOptionsAttribute = fieldInfo.GetCustomAttribute<ReferenceOptionsAttribute>() ?? nullReferenceOptionsAttribute;
        
            if (_PopupStyle == null)
            {
                _PopupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"));
                _PopupStyle.imagePosition = ImagePosition.ImageOnly;
            }
        
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            // Get properties
            SerializedProperty useConstant = property.FindPropertyRelative("UseConstant");
            SerializedProperty constantValue = property.FindPropertyRelative("ConstantValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");

            // Calculate rect for configuration button
            Rect configButtonRect = new Rect(position);
            configButtonRect.yMin += _PopupStyle.margin.top;
            configButtonRect.width = _PopupStyle.fixedWidth + _PopupStyle.margin.right;
            position.xMin = configButtonRect.xMax;

            // Store old indent level and set it to 0, the PrefixLabel takes care of it
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (!referenceOptionsAttribute.ForceVariableOnly)
            {        
                int result = EditorGUI.Popup(configButtonRect, useConstant.boolValue ? 0 : 1, _PopupOption, _PopupStyle);
                useConstant.boolValue = result == 0;
            }
            else
            {
                useConstant.boolValue = false;
            
                //adjust button size / position to make up for not present popup options
                position.x -= configButtonRect.width;
                position.width += configButtonRect.width;
            }

            ScriptableObject createdObject = null;
            if (!useConstant.boolValue)
            {
                InlineManagementDrawer.RenderManagementButtonInline(ref position, variable, property,
                    (pos, prop, includeLabel) => EditorGUI.PropertyField(pos, useConstant.boolValue ? constantValue : prop, GUIContent.none),
                    fieldInfo
                );
            }
        
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}