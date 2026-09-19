using System;
using System;
using UnityEngine;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// Vector3Reference Class.
    /// </summary>
    [Serializable]
    public class Vector3Reference : Reference<Vector3, Vector3Variable>
    {
        public Vector3Reference(Vector3 Value) : base(Value) { }
        public Vector3Reference() { }
    }

    /// <summary>
    /// Vector3Variable Class.
    /// </summary>
#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class Vector3Variable : Variable<Vector3> { }
}