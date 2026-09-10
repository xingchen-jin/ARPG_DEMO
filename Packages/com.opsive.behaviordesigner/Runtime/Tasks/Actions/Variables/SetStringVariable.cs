#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions.Variables
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [Opsive.Shared.Utility.Description("Sets the value of a string SharedVariable by name.")]
    [Shared.Utility.Category("Variables")]
    public class SetStringVariable : SetVariableBase
    {
        [Tooltip("The name of the string SharedVariable.")]
        [SerializeField] protected SharedVariable<string> m_VariableName;
        [Tooltip("The string value to set.")]
        [SerializeField] protected SharedVariable<string> m_Value;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            SetVariableValue(m_VariableName, m_Value.Value);
            return TaskStatus.Success;
        }

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            m_VariableName = "";
            m_Value = "";
        }
    }
}
#endif