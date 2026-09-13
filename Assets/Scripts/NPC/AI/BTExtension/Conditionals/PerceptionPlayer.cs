using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
using UnityEngine;
[Category("Custom/GameObject")]
[Description("判断是否感知到玩家(是否看到,是否有正在追击的玩家)")]
public class PerceptionPlayer : Conditional
{
    public SharedVariable<GameObject> m_TargetGameObject;
    private NPCPerception _npcPerception;
    private BehaviorTree _behaviorTree;
    #region 行为树生命周期方法
    public override void OnAwake()
    {
        _npcPerception = GetComponent<NPCPerception>();
        _behaviorTree = GetComponent<BehaviorTree>();
    }
    /// <summary>
    /// 判断条件为看到玩家或正在追击玩家
    /// </summary>
    /// <returns></returns>
    public override TaskStatus OnUpdate()
    {
        if (CanSeePlayer() || IsGameObjectValid(m_TargetGameObject))
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
    #endregion
    #region 私有方法
    private bool CanSeePlayer()
    {
        if (_npcPerception == null || _npcPerception.DetectedPlayer == null)
        {
            return false;
        }
        GameObject detected = _npcPerception.DetectedPlayer.gameObject;
        //写回节点自己的变量（图中已绑定时它就是共享变量）
        if (m_TargetGameObject != null)
        {
            m_TargetGameObject.Value = detected;
        }
        //再按名字写回行为树共享变量：该节点在图中未绑定时 target 只是节点私有变量，
        //ChaseTarget / 距离判断等节点读取的共享变量会一直是 null，导致追不到目标。
        if (_behaviorTree != null)
        {
            _behaviorTree.SetVariableValue("TargetPlayer", detected);
        }
        return true;
    }
    private bool IsGameObjectValid(SharedVariable<GameObject> targetGameObject)
    {
        if (targetGameObject == null || targetGameObject.Value == null)
        {
            return false;
        }
        return true;
    }
    #endregion
}
