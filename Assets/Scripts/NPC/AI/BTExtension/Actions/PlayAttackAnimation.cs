using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.Shared.Utility;
using UnityEngine;

[Category("Custom/Combat")]
[Description("播放攻击动画，并等待动画播放完毕。动画播放期间返回 Running，阻塞行为树后续节点。")]
public class PlayAttackAnimation : Action
{
    [Tooltip("要播放的动画状态名，需与 Animator Controller 中的 State 名称完全一致。")]
    [SerializeField] protected string m_AnimationStateName = "Attack";

    [Tooltip("动画过渡时间，0 表示瞬间切换。")]
    [SerializeField] protected float m_TransitionDuration = 0.1f;

    [Tooltip("动画所在的 Animator 组件。如果为空，则自动从当前 GameObject 获取。")]
    [SerializeField] protected Animator m_Animator;

    // 内部状态：标记动画是否已经开始播放
    private bool m_IsPlaying;
    // 缓存动画状态的 Hash，避免每帧字符串比较产生 GC
    private int m_AnimationStateHash;

    public override void OnStart()
    {
        base.OnStart();

        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
        }

        if (m_Animator == null)
        {
            Debug.LogError("[PlayAttackAnimation] 未找到 Animator 组件！");
            return;
        }

        m_AnimationStateHash = Animator.StringToHash(m_AnimationStateName);
        m_IsPlaying = true;

        // 使用 CrossFade 实现平滑过渡，避免动画跳帧
        m_Animator.CrossFadeInFixedTime(m_AnimationStateHash, m_TransitionDuration);
    }

    public override TaskStatus OnUpdate()
    {
        if (m_Animator == null || !m_IsPlaying)
        {
            return TaskStatus.Failure;
        }

        // 获取当前动画层（Layer 0）的状态信息
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);

        // 判断条件：
        // 1. 当前处于目标动画状态
        // 2. 不在过渡中（IsInTransition 为 false）
        // 3. 动画播放进度 >= 1.0 (normalizedTime)
        bool isInTargetState = stateInfo.shortNameHash == m_AnimationStateHash;
        bool isNotInTransition = !m_Animator.IsInTransition(0);
        bool isAnimationFinished = stateInfo.normalizedTime >= 1.0f;

        if (isInTargetState && isNotInTransition && isAnimationFinished)
        {
            m_IsPlaying = false;
            return TaskStatus.Success; // 动画播放完毕，返回成功，行为树继续
        }

        return TaskStatus.Running; // 动画播放中，返回 Running，阻塞行为树
    }

    public override void OnEnd()
    {
        base.OnEnd();
        m_IsPlaying = false;
    }

    public override void Reset()
    {
        base.Reset();
        m_Animator = null;
    }
}