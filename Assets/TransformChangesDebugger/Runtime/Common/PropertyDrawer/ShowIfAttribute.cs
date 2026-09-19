using System;
using UnityEngine;

namespace ImmersiveVRTools.Runtime.Common.PropertyDrawer
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
                    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string CheckFieldName { get; }

        public ShowIfAttribute(string checkFieldName)
        {
            this.CheckFieldName = checkFieldName;
        }
    }
}
