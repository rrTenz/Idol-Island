using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.PreferenceDefinition
{
    public class TextProjectEditorPreferenceDefinition : ProjectEditorPreferenceDefinitionBase
    {
        public TextProjectEditorPreferenceDefinition(string label, string preferenceKey, string defaultValue, HandleOnEditorPersistedValueChange handleOnEditorPersistedValueChange = null,
            EditorPreferenceInitialize onInitialize = null)
            : base(label, preferenceKey, defaultValue, handleOnEditorPersistedValueChange, onInitialize)
        {
        }

        public override object GetEditorPersistedValueInternal()
        {
            return EditorPrefs.GetString(PreferenceKey, (string)DefaultValue);
        }

        protected override void SetEditorPersistedValueInternal(object newValue)
        {
            EditorPrefs.SetString(PreferenceKey, (string)newValue);
        }

        public override object RenderEditorAndCaptureInput(object currentValue, GUIStyle style, params GUILayoutOption[] layoutOptions)
        {
            return EditorGUILayout.TextField(GuiContent, currentValue?.ToString() ?? string.Empty, style ?? EditorStyles.textField, layoutOptions);
        }
    }
}