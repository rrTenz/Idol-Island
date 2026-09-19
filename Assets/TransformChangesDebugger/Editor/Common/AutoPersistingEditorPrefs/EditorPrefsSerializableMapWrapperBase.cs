using System;
using System.Collections.Generic;
using System.Linq;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    public interface IEditorPrefsSerializableMapWrapper
    {
        void AddMapEntry(IMapEntry mapEntry);
        List<IMapEntry> GetMapEntries();
    }
    
    [Serializable]
    
    public abstract class EditorPrefsSerializableMapWrapperBase<TMapEntry>: IEditorPrefsSerializableMapWrapper
        where TMapEntry: IMapEntry
    {
        public List<TMapEntry> Map = new List<TMapEntry>();

        protected EditorPrefsSerializableMapWrapperBase(List<TMapEntry> map)
        {
            Map = map;
        }

        public void AddMapEntry(IMapEntry mapEntry)
        {
            Map.Add((TMapEntry)mapEntry);
        }

        public List<IMapEntry> GetMapEntries()
        {
            return Map.Cast<IMapEntry>().ToList();
        }
    }
}