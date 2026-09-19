using System;
using System;
using UnityEngine;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// RectReference Class.
    /// </summary>
    [Serializable]
    public class RectReference : Reference<Rect, RectVariable>
    {
        public RectReference(Rect Value) : base(Value)
        {
        }

        public RectReference()
        {
        }
    }

    /// <summary>
    /// RectVariable Class.
    /// </summary>
#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class RectVariable : Variable<Rect>
    {
    }
}