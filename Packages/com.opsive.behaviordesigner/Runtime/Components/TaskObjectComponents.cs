#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Components
{
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// The DOTS data structure for the TaskObject class.
    /// </summary>
    public struct TaskObjectComponent : IBufferElementData
    {
        [Tooltip("The index of the task.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS flag indicating when a TaskObject node is active.
    /// </summary>
    public struct TaskObjectFlag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// A DOTS tag indicating when a TaskObject node needs to be reevaluated.
    /// </summary>
    public struct TaskObjectReevaluateFlag : IComponentData, IEnableableComponent
    {
    }
}
#endif
