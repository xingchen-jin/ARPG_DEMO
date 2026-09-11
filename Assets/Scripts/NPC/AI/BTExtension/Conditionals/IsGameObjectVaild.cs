using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
using UnityEngine;

[Category("Custom/GameObject")]                  
[Description("判断 SharedVariable<GameObject> 是否为空。变量本身或 Value 为 null 时返回 Failure。")]
public class IsGameObjectVaild : Conditional
{
    [Tooltip("要检查的 SharedVariable<GameObject> 变量。")]
    public SharedVariable<GameObject> m_TargetGameObject;

    public override TaskStatus OnUpdate()
    {
        if (m_TargetGameObject == null || m_TargetGameObject.Value == null)
        {
            return TaskStatus.Failure;
        }
        return TaskStatus.Success;
    }
    /// <summary>
    /// 重置回调函数，将 SharedVariable<GameObject> 变量为 null。
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        m_TargetGameObject = null;
    }
}
