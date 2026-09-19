using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public class DedicatedInspectorWindow 
    {
        private static readonly Type InspectorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        private static readonly MethodInfo IsLockedSetter = InspectorType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public).GetSetMethod();

        //TODO: check in unity 2018 / 19 / 20 / 21?
        public static void InspectTarget(UnityEngine.Object target, string instanceName, Vector2? windowMinSize = null)
        {
            var allInspectors = Resources.FindObjectsOfTypeAll(InspectorType);
            var inspectorInstance = allInspectors.FirstOrDefault(i => i.name == instanceName) as EditorWindow;
            Vector2? prevMinSize = null;
            Vector2? prevMaxSize = null;
            if (!inspectorInstance)
            {
                inspectorInstance = ScriptableObject.CreateInstance(InspectorType) as EditorWindow;
                inspectorInstance.name = instanceName;
                
                prevMinSize = inspectorInstance.minSize;
                prevMaxSize = inspectorInstance.maxSize;
                
                if (windowMinSize.HasValue)
                {
                    inspectorInstance.minSize = windowMinSize.Value;
                    inspectorInstance.maxSize = windowMinSize.Value;
                }
            }

            inspectorInstance.Show();

            if(prevMinSize.HasValue) inspectorInstance.minSize = prevMinSize.Value;
            if(prevMaxSize.HasValue) inspectorInstance.maxSize = prevMaxSize.Value;
            
            var prevSelection = Selection.activeObject;
            
            IsLockedSetter.Invoke(inspectorInstance, new object[] { false });
            Selection.activeObject = target;
            IsLockedSetter.Invoke(inspectorInstance, new object[] { true });

            Selection.activeObject = prevSelection;
        }
    }
}