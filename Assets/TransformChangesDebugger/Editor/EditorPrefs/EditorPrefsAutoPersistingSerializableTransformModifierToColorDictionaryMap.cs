using System;
using System.Collections.Generic;
using ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs;
using TransformChangesDebugger.API;
using UnityEngine;

namespace TransformChangesDebugger.Editor
{
    [Serializable]
    internal class SerializableTransformModifier: ISerializableTransformModifier
    {
        public string CallingObjectFullPath => _CallingObjectFullPath;
        public string CallingFromMethodName => _CallingFromMethodName;
        
        public string _CallingObjectFullPath;
        public string _CallingFromMethodName;
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            
            var otherModifier = obj as ISerializableTransformModifier;
            return otherModifier != null 
                   && otherModifier.CallingObjectFullPath == this.CallingObjectFullPath
                   && otherModifier.CallingFromMethodName == this.CallingFromMethodName;
        }
        
        public override int GetHashCode()
        {
            return _CallingObjectFullPath.GetHashCode() + _CallingFromMethodName.GetHashCode();
        }

        public SerializableTransformModifier(string callingObjectFullPath, string callingFromMethodName)
        {
            this._CallingObjectFullPath = callingObjectFullPath;
            _CallingFromMethodName = callingFromMethodName;
        }
    }
    
    [Serializable]
    internal class EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap: EditorPrefsAutoPersistingMapBase<ISerializableTransformModifier, Color, 
        EditorPrefsSerializableTransformModifierToColorDictionaryMapEntryMapWrapper, SerializableTransformModifierToColorDictionaryMapEntry>
    {
        public EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap(string editorPrefsKey, Dictionary<ISerializableTransformModifier, Color> initialIfNotAlreadyPersisted) : base(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
        }

        public EditorPrefsAutoPersistingSerializableTransformModifierToColorDictionaryMap(string editorPrefsKey, Dictionary<ISerializableTransformModifier, Color> initialIfNotAlreadyPersisted, EventHandler<SerializableTransformModifierToColorDictionaryMapEntry> onValueChanged) : base(editorPrefsKey, initialIfNotAlreadyPersisted, onValueChanged)
        {
        }
    }
    
    [Serializable]
    internal class EditorPrefsSerializableTransformModifierToColorDictionaryMapEntryMapWrapper: EditorPrefsSerializableMapWrapperBase<SerializableTransformModifierToColorDictionaryMapEntry>
    {
        public EditorPrefsSerializableTransformModifierToColorDictionaryMapEntryMapWrapper(List<SerializableTransformModifierToColorDictionaryMapEntry> map) : base(map)
        {
        }

        public EditorPrefsSerializableTransformModifierToColorDictionaryMapEntryMapWrapper(): base(new List<SerializableTransformModifierToColorDictionaryMapEntry>())
        {

        }
    }

    [Serializable]
    internal class SerializableTransformModifierToColorDictionaryMapEntry: MapEntryBase<SerializableTransformModifier, Color>
    {
        public SerializableTransformModifierToColorDictionaryMapEntry(SerializableTransformModifier key, Color value) : base(key, value)
        {
        }

        public SerializableTransformModifierToColorDictionaryMapEntry() 
            : base(new SerializableTransformModifier("None", "None"), Color.clear)
        {
        }

        public override void SetValues(object key, object value)
        {
            var keyAsInterface = (ISerializableTransformModifier)key;
            
            Key = new SerializableTransformModifier(keyAsInterface.CallingObjectFullPath, keyAsInterface.CallingFromMethodName);
            Value = (Color) value;
        }
    } 
}