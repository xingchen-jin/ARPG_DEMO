using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
[Category("Custom/AI")]                  
[Description("判断 NPC 是否能看到目标。")]
public class CanSeeTarget : Conditional
{
    public SharedVariable<GameObject> target;
    private NPCPerception _npcPerception;
    #region 行为树生命周期方法
    public override void OnAwake()
    {
        _npcPerception = GetComponent<NPCPerception>();
    }
    public override TaskStatus OnUpdate()
    {
//        Debug.Log("开始判断");
        if( _npcPerception != null && _npcPerception.DetectedPlayer != null)
        {
    //        Debug.Log($"NPC {gameObject.name} 检测到玩家: {_npcPerception.DetectedPlayer.gameObject.name}");
            target.Value = _npcPerception.DetectedPlayer.gameObject;
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;  
    }
    #endregion
}