#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    /// <summary>
    /// Specifies that the node is an object task which can specify the next child that should run.
    /// </summary>
    public interface ITaskObjectParentNode
    {
        /// <summary>
        /// Returns the index of the next child that should run. Set to ushort.MaxValue to ignore.
        /// </summary>
        ushort NextChildIndex { get; }
    }
}
#endif
