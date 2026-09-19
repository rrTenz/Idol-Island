using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.PropertyDrawer
{
    public class InlineManagementAttribute: PropertyAttribute
    {
        public string AfterCreationNewRunMethodName { get; set; } 
        public bool IsCreatingNewDisabled { get; set; }
    }
}