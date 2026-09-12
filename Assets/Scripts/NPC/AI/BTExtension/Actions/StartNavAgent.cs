using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.Shared.Utility;

[Category("Custom/AI")]
[Description("开始 NavMeshAgent 的移动。")]
public class StartNavAgent : Action
{
    private UnityEngine.AI.NavMeshAgent _agent;

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

        _agent.isStopped = false;
        return TaskStatus.Success;
    }
}
