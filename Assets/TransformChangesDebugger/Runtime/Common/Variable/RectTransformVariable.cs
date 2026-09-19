using System;
using System;
using UnityEngine;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// RectTransformReference Class.
    /// </summary>
    [Serializable]
    public class RectTransformReference : Reference<RectTransform, RectTransformVariable>
    {
        public RectTransformReference(RectTransform Value) : base(Value)
        {
        }

        public RectTransformReference()
        {
        }
    }

    /// <summary>
    /// RectTransformVariable Class.
    /// </summary>
#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class RectTransformVariable : Variable<RectTransform>
    {
    }
}