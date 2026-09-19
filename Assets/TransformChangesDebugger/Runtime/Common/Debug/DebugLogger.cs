using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Debug
{
    public class DebugLogger : MonoBehaviour
    {
        [SerializeField] private string MessagePrefix = "CustomDebug: ";

        public void Log(string message)
        {
            UnityEngine.Debug.Log(MessagePrefix + message);
        }
    }
}