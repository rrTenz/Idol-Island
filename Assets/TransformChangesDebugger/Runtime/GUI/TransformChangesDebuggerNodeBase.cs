using System;
using XNode;

namespace TransformChangesDebugger.Runtime.GUI
{
    [Serializable]
    [Node.NodeWidthAttribute(TransformChangesDebuggerNodeBase.NodeWidth)]
    public abstract class TransformChangesDebuggerNodeBase :AutoAligningNodeBase
    {

    }

    public abstract class AutoAligningNodeBase: XNode.Node
    {
        public const int NodeWidth = 300;
        
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
    }
}