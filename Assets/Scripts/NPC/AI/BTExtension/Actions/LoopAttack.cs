using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.Shared.Utility;
using UnityEngine;


[Category("Custom/AI")]
[Description("循环攻击动作，持续攻击直到条件不满足。")]
public class LoopAttack : Action
{
    [Header("攻击判断参数")]
    [SerializeField] protected SharedVariable<GameObject> m_SelfGameObject;
    [SerializeField] protected SharedVariable<GameObject> m_TargetGameObject;
    [SerializeField] protected SharedVariable<float> m_AttackDistance;

    [Header("动画参数")]
    [SerializeField] protected SharedVariable<string> m_AttackAnimationName;
    [SerializeField] protected SharedVariable<float>  m_TransitionDuration = 0.25f;
    [SerializeField] protected SharedVariable<int> m_Layer = 0;
    private Transform m_SelfTransform;
    private Transform m_TargetTransform;

    private Animator m_Animator;
    private bool m_IsAttacking = false;
    private int m_AnimationHash;

    #region 行为树生命周期方法
    public override void OnAwake()
    {
        //如果没有指定_selfGameObject，则使用当前GameObject
        if (m_SelfGameObject == null || m_SelfGameObject.Value == null)
        {
            m_SelfGameObject = m_GameObject;
        }
        //获取自身组件
        m_Animator = m_SelfGameObject.Value.GetComponent<Animator>();

        //存储Transform引用以提高性能
        m_SelfTransform = m_SelfGameObject.Value.transform;

        m_IsAttacking = false;  
        m_AnimationHash = Animator.StringToHash(m_AttackAnimationName.Value);
    }
    public override TaskStatus OnUpdate()
    {
        m_TargetTransform = m_TargetGameObject != null && m_TargetGameObject.Value != null ? m_TargetGameObject.Value.transform : null;
        // 检查必要的组件是否存在
        if (m_Animator == null || m_TargetGameObject == null || m_TargetGameObject.Value == null 
        || m_SelfTransform == null || m_TargetTransform == null)
        {
            Debug.LogWarning($"必要的组件未找到 传入列表:\n 自身gameobject：{m_SelfGameObject?.Value?.name ?? "null"}\n 目标gameobject：{m_TargetGameObject?.Value?.name ?? "null"}\n自身Transform: {m_SelfTransform?.name ?? "null"}\n 目标Transform: {m_TargetTransform?.name ?? "null"}");
            return TaskStatus.Failure;
        }

        // 检查攻击距离
        float distance = Vector3.Distance(m_SelfTransform.position, m_TargetTransform.position);
        if (distance > m_AttackDistance.Value)
        {
            return TaskStatus.Success;
        }

       // 动画循环逻辑
        if (!m_IsAttacking)
        {
            m_Animator.CrossFadeInFixedTime(m_AnimationHash, m_TransitionDuration.Value, m_Layer.Value);
            m_IsAttacking = true;
        }
        else
        {
            // 等待当前动画播完，播完立刻重播
            if (!m_Animator.IsInTransition(m_Layer.Value))
            {
                AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(m_Layer.Value);
                if (stateInfo.shortNameHash == m_AnimationHash && stateInfo.normalizedTime >= 1.0f)
                {
                    m_Animator.CrossFadeInFixedTime(m_AnimationHash, m_TransitionDuration.Value, m_Layer.Value);
                }
            }
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        m_IsAttacking = false;

    }
    #endregion
    
}
