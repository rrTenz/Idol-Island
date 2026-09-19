using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.PreferenceDefinition
{
    public class IntProjectEditorPreferenceDefinition : ProjectEditorPreferenceDefinitionBase
    {
        public IntProjectEditorPreferenceDefinition(string label, string preferenceKey, int defaultValue, HandleOnEditorPersistedValueChange handleOnEditorPersistedValueChange = null, 
            EditorPreferenceInitialize onInitialize = null)
            : base(label, preferenceKey, defaultValue, handleOnEditorPersistedValueChange, onInitialize )
        {
        }

        public override object GetEditorPersistedValueInternal()
        {
            return EditorPrefs.GetInt(PreferenceKey, int.Parse(DefaultValue.ToString()));
        }

        protected override void SetEditorPersistedValueInternal(object newValue)
        {
            EditorPrefs.SetInt(PreferenceKey, (int)newValue);
        }

        public override object RenderEditorAndCaptureInput(object currentValue, GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            var value = (int)currentValue == 0 ? (int)DefaultValue : (int)currentValue;
            return EditorGUILayout.IntField(GuiContent, value, style ?? EditorStyles.textField, layoutOptions);
        }
    }
}