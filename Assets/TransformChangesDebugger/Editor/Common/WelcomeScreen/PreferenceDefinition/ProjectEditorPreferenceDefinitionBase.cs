using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.WelcomeScreen.PreferenceDefinition
{
    public abstract class ProjectEditorPreferenceDefinitionBase
    {
        public string Label { get; }

        private string _rawPreferenceKey;
        private readonly EditorPreferenceInitialize _onInitialize;
        private string _preferenceKey;
        public string PreferenceKey => _preferenceKey ?? (_preferenceKey = IsGlobal
            ? $"{_globalPerfProductName}_{_rawPreferenceKey}"
            : $"{Application.productName}_{_rawPreferenceKey}");

        public object DefaultValue { get; }
        public HandleOnEditorPersistedValueChange HandleOnEditorPersistedValueChange { get; }

        private GUIContent _guiContent;
        protected GUIContent GuiContent => _guiContent ?? (_guiContent = new GUIContent(Label));

        private bool IsGlobal => !string.IsNullOrWhiteSpace(_globalPerfProductName);
        private string _globalPerfProductName;

        public ProjectEditorPreferenceDefinitionBase(string label, string preferenceKey, object defaultValue,
            HandleOnEditorPersistedValueChange handleOnEditorPersistedValueChange = null, EditorPreferenceInitialize onInitialize = null)
        {
            Label = label;
            _rawPreferenceKey = preferenceKey;
            _onInitialize = onInitialize;
            DefaultValue = defaultValue;
            HandleOnEditorPersistedValueChange = handleOnEditorPersistedValueChange;
        }

        public abstract object GetEditorPersistedValueInternal();
        protected abstract void SetEditorPersistedValueInternal(object newValue);

        public void Initialize()
        {
            _onInitialize?.Invoke(GetEditorPersistedValueOrDefault());
        }
        
        public object GetEditorPersistedValueOrDefault()
        {
            if (!EditorPrefs.HasKey(PreferenceKey))
            {
                SetEditorPersistedValueInternal(DefaultValue);
                HandleOnEditorPersistedValueChange?.Invoke(DefaultValue, null);
                return DefaultValue;
            }
            return GetEditorPersistedValueInternal();
        }

        public void SetEditorPersistedValue(object newValue)
        {
            var oldValue = GetEditorPersistedValueOrDefault();
            SetEditorPersistedValueInternal(newValue);
            HandleOnEditorPersistedValueChange?.Invoke(newValue, oldValue);
        }

        public T AsGlobal<T>(string globalProductName) where T : ProjectEditorPreferenceDefinitionBase
        {
            _globalPerfProductName = globalProductName;
            return (T)this;
        }

        public abstract object RenderEditorAndCaptureInput(object currentValue, GUIStyle style, params GUILayoutOption[] layoutOptions);
    }
}