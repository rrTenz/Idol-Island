using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// GameObjectListReference Class.
    /// </summary>
    [Serializable]
    public class GameObjectListReference : Reference<List<GameObject>, GameObjectListVariable>
    {
        public GameObjectListReference(List<GameObject> Value) : base(Value)
        {
        }

        public GameObjectListReference()
        {
        }
    }

    /// <summary>
    /// GameObjectListVariable Class.
    /// </summary>

#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class GameObjectListVariable : Variable<List<GameObject>>
    {
    }
}