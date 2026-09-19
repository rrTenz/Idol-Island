using System;
using System.Collections.Generic;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    [Serializable]
    public class EditorPrefsAutoPersistingStringToBoolDictionaryMap: EditorPrefsAutoPersistingMapBase<string, bool, 
        EditorPrefsSerializableStringToBoolDictionaryMapWrapper, StringToBoolDictionaryMapEntry>
    {
        public EditorPrefsAutoPersistingStringToBoolDictionaryMap(string editorPrefsKey, Dictionary<string, bool> initialIfNotAlreadyPersisted)
            : base(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
        }
    }

    [Serializable]
    public class EditorPrefsSerializableStringToBoolDictionaryMapWrapper: EditorPrefsSerializableMapWrapperBase<StringToBoolDictionaryMapEntry>
    {
        public EditorPrefsSerializableStringToBoolDictionaryMapWrapper(List<StringToBoolDictionaryMapEntry> map) : base(map)
        {
        }

        public EditorPrefsSerializableStringToBoolDictionaryMapWrapper(): base(new List<StringToBoolDictionaryMapEntry>())
        {

        }
    }

    [Serializable]
    public class StringToBoolDictionaryMapEntry: MapEntryBase<string, bool>
    {
        public StringToBoolDictionaryMapEntry(string key, bool value) : base(key, value)
        {
        }

        public StringToBoolDictionaryMapEntry(): base(string.Empty, false)
        {

        }
    }   
}