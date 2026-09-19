using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.AutoPersistingEditorPrefs
{
    public abstract class EditorPrefsAutoPersistingMapBase<TKey, TValue, TWrapper, TMapEntry>
        where TWrapper: class, IEditorPrefsSerializableMapWrapper, new()
        where TMapEntry: class, IMapEntry, new()
    {
        private string _editorPrefsKey;
        private readonly Dictionary<TKey, TValue> _initialIfNotAlreadyPersisted;
        private Dictionary<TKey, TValue> _internalKeyToValueMap = new Dictionary<TKey, TValue>();
        public event EventHandler<TMapEntry> ValueChanged;
        
        public EditorPrefsAutoPersistingMapBase(string editorPrefsKey, Dictionary<TKey, TValue> initialIfNotAlreadyPersisted)
        {
            _editorPrefsKey = editorPrefsKey;
            _initialIfNotAlreadyPersisted = initialIfNotAlreadyPersisted;
            Initialize();

            if (_internalKeyToValueMap.Count == 0)
            {
                foreach (var kv in initialIfNotAlreadyPersisted)
                {
                    _internalKeyToValueMap[kv.Key] = kv.Value;
                }
            }
        }

        public EditorPrefsAutoPersistingMapBase(string editorPrefsKey, Dictionary<TKey, TValue> initialIfNotAlreadyPersisted, EventHandler<TMapEntry> onValueChanged)
            :this(editorPrefsKey, initialIfNotAlreadyPersisted)
        {
            ValueChanged += onValueChanged;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (!_internalKeyToValueMap.ContainsKey(key))
                {
                    //when new entries are added this will make sure that object is correctly populated with initial values, this will allow to add new values ad-hoc without
                    //the need to wipe existing setting persisted in registry
                    if (_initialIfNotAlreadyPersisted.TryGetValue(key, out var initialValue))
                    {
                        _internalKeyToValueMap[key] = initialValue;
                        PersistChanges();
                    }
                    else
                    {
                        throw new Exception($"Unable to find key: {key} for auto persisting editor prefs, make sure this is added to initial values");
                    }
                }
                
                return _internalKeyToValueMap[key];
            }
            set
            {
                var hasPreviousValue = _internalKeyToValueMap.TryGetValue(key, out var previousValue);
                _internalKeyToValueMap[key] = value;

                if (!hasPreviousValue || !previousValue.Equals(value))
                {
                    var wrapper = PersistChanges();
                    var changedMapEntry = wrapper.GetMapEntries().First(m => m.GetKey().Equals(key));
                    ValueChanged?.Invoke(this, (TMapEntry)changedMapEntry);
                }
            }
        }

        public void Remove(TKey key)
        {
            _internalKeyToValueMap.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return _internalKeyToValueMap.ContainsKey(key);
        }
        
        public KeyValuePair<TKey, TValue> FirstOrDefault(Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            return _internalKeyToValueMap.FirstOrDefault(predicate);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _internalKeyToValueMap.TryGetValue(key, out value);
        }

        public TWrapper PersistChanges()
        {
            var wrapper = new TWrapper();

            var mapEntries = _internalKeyToValueMap.Select(kv =>
            {
                var mapEntry = new TMapEntry();
                mapEntry.SetValues(kv.Key, kv.Value);
                return mapEntry;
            }).ToList();
            mapEntries.ForEach(m => wrapper.AddMapEntry(m));
            EditorPrefs.SetString(_editorPrefsKey, JsonUtility.ToJson(wrapper));

            return wrapper;
        }

        private void Initialize()
        {
            var stringToSerialize = EditorPrefs.GetString(_editorPrefsKey);
            if (string.IsNullOrEmpty(stringToSerialize)) stringToSerialize = "{}";

            var serializedObject = JsonUtility.FromJson<TWrapper>(stringToSerialize);

            List<IMapEntry> mapEntries;
            if (serializedObject != null && (mapEntries = serializedObject.GetMapEntries())?.Count > 0)
            {
                foreach (var mapEntry in mapEntries)
                {
                    _internalKeyToValueMap[(TKey)mapEntry.GetKey()] = (TValue)mapEntry.GetValue();
                }
            }
        }
    }
}