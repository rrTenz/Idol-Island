using System;

namespace ImmersiveVRTools.Runtime.Common.PropertyDrawer
{
    public class ReferenceOptionsAttribute: Attribute
    {
        public bool ForceVariableOnly { get; set; }
        public string AfterCreationNewRunMethodName { get; set; } 
    }
}