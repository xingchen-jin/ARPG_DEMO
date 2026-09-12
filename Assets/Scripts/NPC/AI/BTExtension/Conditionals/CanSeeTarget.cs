using UnityEngine;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
[Category("Custom/AI")]
[Description("判断 NPC 是否能看到目标，并把目标写入行为树的共享变量，供 ChaseTarget、距离判断等节点读取。")]
public class CanSeeTarget : Conditional
{
    [Tooltip("节点自身的变量，在图中绑定共享变量后与行为树变量是同一个对象。")]
    public SharedVariable<GameObject> target;

    [Tooltip("行为树中保存当前目标的共享变量名，追逐、距离判断等节点都读取它。")]
    [SerializeField] private string targetVariableName = "TargetPlayer";

    private NPCPerception _npcPerception;
    private BehaviorTree _behaviorTree;
    #region 行为树生命周期方法
    public override void OnAwake()
    {
        _npcPerception = GetComponent<NPCPerception>();
        _behaviorTree = GetComponent<BehaviorTree>();
    }
    public override TaskStatus OnUpdate()
    {
        if( _npcPerception == null || _npcPerception.DetectedPlayer == null)
        {
            return TaskStatus.Failure;
        }

        GameObject detected = _npcPerception.DetectedPlayer.gameObject;

        //写回节点自己的变量（图中已绑定时它就是共享变量）
        if (target != null)
        {
            target.Value = detected;
        }

        //再按名字写回行为树共享变量：该节点在图中未绑定时 target 只是节点私有变量，
        //ChaseTarget / 距离判断等节点读取的共享变量会一直是 null，导致追不到目标。
        if (_behaviorTree != null)
        {
            _behaviorTree.SetVariableValue(targetVariableName, detected);
        }

        return TaskStatus.Success;
    }
    #endregion
}
