namespace ImmersiveVRTools.Runtime.Common.Variable
{
    /// <summary>
    /// Variable Class.
    /// </summary>
    public class Variable<T> : UnityEngine.ScriptableObject
    {
        public T Value;

        public void SetValue(T value)
        {
            Value = value;
        }

        public void SetValue(Variable<T> value)
        {
            Value = value.Value;
        }
    }
}