using System;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    public interface IMapEntry
    {
        void SetValues(object key, object value);
        object GetKey();
        object GetValue();
    }
    
    [Serializable]
    public abstract class MapEntryBase<TKey, TValue>: IMapEntry
    {
        public TKey Key;
        public TValue Value;

        public MapEntryBase(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }


        public virtual void SetValues(object key, object value)
        {
            Key = (TKey) key;
            Value = (TValue) value;
        }

        public object GetKey()
        {
            return Key;
        }

        public object GetValue()
        {
            return Value;
        }
    }
}