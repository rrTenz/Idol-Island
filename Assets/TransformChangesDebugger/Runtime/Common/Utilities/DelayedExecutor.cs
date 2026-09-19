using System;
using System.Collections;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.Utilities
{
    public class DelayedExecutor
    {
        public static IEnumerator ExecuteOnNextFrame(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }
    }
}