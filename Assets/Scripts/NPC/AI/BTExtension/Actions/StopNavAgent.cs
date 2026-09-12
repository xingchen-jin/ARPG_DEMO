using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
using UnityEngine;

[Category("Custom/AI")]
[Description("停止 NavMeshAgent 的移动。")]
public class StopNavAgent : Action
{
    private UnityEngine.AI.NavMeshAgent _agent;

    [Tooltip("是否清空原先的路径")]
    public SharedVariable<bool> clearPath = true;
    public override void OnAwake()
    {
        _agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_agent == null)
        {
            return TaskStatus.Failure;
        }
        //停止移动
        _agent.isStopped = true;
        // 如果需要清空路径，则调用 ResetPath 方法
        if(clearPath != null && clearPath.Value)
        {
            _agent.ResetPath();
        }

        return TaskStatus.Success;
    }
}
