using System;
using System.Collections.Generic;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    [Serializable]
    public class EditorPrefsAutoPersistingEnumToBoolDictionaryMap: EditorPrefsAutoPersistingMapBase<int, bool, 
        EditorPrefsSerializableEnumToBoolDictionaryMapWrapper, EnumToBoolDictionaryMapEntry>
    {
        public EditorPrefsAutoPersistingEnumToBoolDictionaryMap(string editorPrefsKey, Dictionary<int, bool> initialIfNotAlreadyPersisted)
            : base(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
        }

        public EditorPrefsAutoPersistingEnumToBoolDictionaryMap(string editorPrefsKey, Dictionary<int, bool> initialIfNotAlreadyPersisted, EventHandler<EnumToBoolDictionaryMapEntry> onValueChanged) : base(editorPrefsKey, initialIfNotAlreadyPersisted, onValueChanged)
        {
        }
    }
    
    [Serializable]
    public class EditorPrefsSerializableEnumToBoolDictionaryMapWrapper: EditorPrefsSerializableMapWrapperBase<EnumToBoolDictionaryMapEntry>
    {
        public EditorPrefsSerializableEnumToBoolDictionaryMapWrapper(List<EnumToBoolDictionaryMapEntry> map) : base(map)
        {
        }

        public EditorPrefsSerializableEnumToBoolDictionaryMapWrapper(): base(new List<EnumToBoolDictionaryMapEntry>())
        {

        }
    }

    [Serializable]
    public class EnumToBoolDictionaryMapEntry: MapEntryBase<int, bool>
    {
        public EnumToBoolDictionaryMapEntry(int key, bool value) : base(key, value)
        {
        }

        public EnumToBoolDictionaryMapEntry(): base(0, false)
        {

        }
    }   
}