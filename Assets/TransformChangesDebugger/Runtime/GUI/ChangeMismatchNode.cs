using System;
using TransformChangesDebugger.API.Patches;

namespace TransformChangesDebugger.Runtime.GUI
{
    [Serializable]
    public class ChangeMismatchNode : TransformChangesDebuggerNodeBase
    {
        public object ExpectedValue { get; set; }
        public object ActualValue { get; set; }
        public bool RenderWarningAsLabel { get; set; }
        public ChangeType ChangeType { get; set; }
    }
}