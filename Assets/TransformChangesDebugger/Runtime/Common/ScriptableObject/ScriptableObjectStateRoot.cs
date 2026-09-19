using System;
using System.Collections.Generic;

namespace ImmersiveVRTools.Runtime.Common.ScriptableObject
{
    public class ScriptableObjectStateRoot
    {
        private readonly Dictionary<object, Dictionary<Type, ScriptableObjectState>> _ownerObjectToStateMap = new Dictionary<object, Dictionary<Type, ScriptableObjectState>>();
        
        public T Get<T>(object owner) where T: ScriptableObjectState, new()
        {
            if (!_ownerObjectToStateMap.ContainsKey(owner))
            {
                _ownerObjectToStateMap.Add(owner, new Dictionary<Type, ScriptableObjectState>());
            }

            var scriptableObjectStates = _ownerObjectToStateMap[owner];
            if (!scriptableObjectStates.ContainsKey(typeof(T)))
            {
                scriptableObjectStates.Add(typeof(T), new T());
            }
            
            return (T)scriptableObjectStates[typeof(T)];
        }
    }
}