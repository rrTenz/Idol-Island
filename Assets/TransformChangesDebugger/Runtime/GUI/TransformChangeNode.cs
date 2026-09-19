using System;
using TransformChangesDebugger.API;
using UnityEngine;
using Component = UnityEngine.Component;

namespace TransformChangesDebugger.Runtime.GUI
{
    [Serializable]
    public class OriginalMethodCall: UnityEngine.Object
    {
        public string Name;
        public object[] Arguments;

        public OriginalMethodCall(string name, object[] arguments)
        {
            Name = name;
            Arguments = arguments;
        }
    }
    
    [Serializable]
    public abstract class TransformChangeNode : TransformChangesDebuggerNodeBase {
        public Component Originator;
        public string MethodName;
        
        public OriginalMethodCall OriginalMethodCall { get; set; }
        public TrackTransformChanges ModifiedObject { get; set; }
        public ComparisonOnlyTransformModifier ComparisonOnlyTransformModifier { get; set; }
        public bool IsOriginalMethodCallFoldoutExpanded { get; set; }

        // public ChangeReason ChangeReason;
        public abstract void SetChangeToModifiedObject();

        public bool IsModifiedVia(TransformModifier transformModifier)
        {
            return ComparisonOnlyTransformModifier.Equals(transformModifier);
        }
    }

    [Serializable]
    public class PositionTransformChangeNode : TransformChangeNode
    {
        public Vector3 Position;
        
        public override void SetChangeToModifiedObject()
        {
            ModifiedObject.transform.position = Position;
        }
    }
    
    [Serializable]
    public class ScaleTransformChangeNode : TransformChangeNode
    {
        public Vector3 Scale;

        public override void SetChangeToModifiedObject()
        {
            ModifiedObject.transform.localScale = Scale;
        }
    }
    
    [Serializable]
    public class RotationTransformChangeNode : TransformChangeNode
    {
        public Quaternion Rotation;

        
        public override void SetChangeToModifiedObject()
        {
            ModifiedObject.transform.rotation = Rotation;
        }
    }
}