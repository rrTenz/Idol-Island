using System;
using System;

namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// StringReference Class.
    /// </summary>
    [Serializable]
    public class StringReference : Reference<string, StringVariable>
    {
        public StringReference(string Value) : base(Value) { }
        public StringReference() { }
    }

    /// <summary>
    /// StringVariable Class.
    /// </summary>
#if ScriptableObjectVariablesMenuCreationEnabled
[CreateAssetMenu]
#endif
    public class StringVariable : Variable<string> { }
}