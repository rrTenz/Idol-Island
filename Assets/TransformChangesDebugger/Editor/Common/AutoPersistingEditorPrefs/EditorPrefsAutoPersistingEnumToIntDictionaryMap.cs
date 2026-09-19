using System;
using System.Collections.Generic;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    [Serializable]
    public class EditorPrefsAutoPersistingEnumToIntDictionaryMap: EditorPrefsAutoPersistingMapBase<int, int, 
        EditorPrefsSerializableEnumToIntDictionaryMapWrapper, EnumToIntDictionaryMapEntry>
    {
        public EditorPrefsAutoPersistingEnumToIntDictionaryMap(string editorPrefsKey, Dictionary<int, int> initialIfNotAlreadyPersisted)
            : base(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
        }
    }
    
    [Serializable]
    public class EditorPrefsSerializableEnumToIntDictionaryMapWrapper: EditorPrefsSerializableMapWrapperBase<EnumToIntDictionaryMapEntry>
    {
        public EditorPrefsSerializableEnumToIntDictionaryMapWrapper(List<EnumToIntDictionaryMapEntry> map) : base(map)
        {
        }

        public EditorPrefsSerializableEnumToIntDictionaryMapWrapper(): base(new List<EnumToIntDictionaryMapEntry>())
        {

        }
    }

    [Serializable]
    public class EnumToIntDictionaryMapEntry: MapEntryBase<int, int>
    {
        public EnumToIntDictionaryMapEntry(int key, int value) : base(key, value)
        {
        }

        public EnumToIntDictionaryMapEntry(): base(0, 0)
        {

        }
    }   
}