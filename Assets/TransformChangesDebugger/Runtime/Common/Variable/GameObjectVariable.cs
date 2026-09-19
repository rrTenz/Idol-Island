using System;
using System;
using UnityEngine;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// GameObjectReference Class.
    /// </summary>
    [Serializable]
    public class GameObjectReference : Reference<GameObject, GameObjectVariable>
    {
        public GameObjectReference(GameObject Value) : base(Value)
        {
        }

        public GameObjectReference()
        {
        }
    }

    /// <summary>
    /// GameObjectVariable Class.
    /// </summary>
#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class GameObjectVariable : Variable<GameObject>
    {
    }
}