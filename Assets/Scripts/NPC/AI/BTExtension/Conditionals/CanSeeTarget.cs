using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
public class CanSeeTarget : Conditional
{
    public SharedVariable<Transform> target;
    private NPCPerception _npcPerception;
    #region 行为树生命周期方法
    public override void OnAwake()
    {
        _npcPerception = GetComponent<NPCPerception>();
    }
    public override TaskStatus OnUpdate()
    {
        Debug.Log("开始判断");
        if( _npcPerception != null && _npcPerception.DetectedPlayer != null)
        {
            Debug.Log($"NPC {gameObject.name} 检测到玩家: {_npcPerception.DetectedPlayer.gameObject.name}");
            target.Value = _npcPerception.DetectedPlayer;
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;  
    }
    #endregion
}