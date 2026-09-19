using System;
using System.Collections.Generic;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    [Serializable]
    public class EditorPrefsAutoPersistingStringToIntDictionaryMap: EditorPrefsAutoPersistingMapBase<string, int, 
        EditorPrefsSerializableStringToIntDictionaryMapWrapper, StringToIntDictionaryMapEntry>
    {
        public EditorPrefsAutoPersistingStringToIntDictionaryMap(string editorPrefsKey, Dictionary<string, int> initialIfNotAlreadyPersisted)
            : base(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
        }
    }
    
    [Serializable]
    public class EditorPrefsSerializableStringToIntDictionaryMapWrapper: EditorPrefsSerializableMapWrapperBase<StringToIntDictionaryMapEntry>
    {
        public EditorPrefsSerializableStringToIntDictionaryMapWrapper(List<StringToIntDictionaryMapEntry> map) : base(map)
        {
        }

        public EditorPrefsSerializableStringToIntDictionaryMapWrapper(): base(new List<StringToIntDictionaryMapEntry>())
        {

        }
    }

    [Serializable]
    public class StringToIntDictionaryMapEntry: MapEntryBase<string, int>
    {
        public StringToIntDictionaryMapEntry(string key, int value) : base(key, value)
        {
        }

        public StringToIntDictionaryMapEntry(): base(string.Empty, 0)
        {

        }
    }   
}