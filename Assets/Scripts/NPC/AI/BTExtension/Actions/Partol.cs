using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using UnityEngine.AI;
using Opsive.Shared.Utility;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions.AnimatorTasks;

[Category("Custom/AI")]                  
[Description("NPC 巡逻行为，使用 NavMeshAgent 进行移动。")]
public class Partol : Action
{
    [Header("巡逻参数")]
    [Tooltip("巡逻半径")]
    public SharedVariable<float> patrolRadius = 10f;
    [Tooltip("巡逻结束等待时间")]
    public SharedVariable<float> waitTime = 5f;
    [Tooltip("到达目标点的判定距离")]
    public SharedVariable<float> arrivalDistance = 1f;
    [Tooltip("可选的巡逻中心点，留空则使用自身位置")]
    public SharedVariable<Transform> patrolCenter;

    // [Header("动画参数")]
    // [Tooltip("要播放的动画状态名")]
    // [SerializeField] protected string m_AnimationStateName = "Locomotion";
    // [Tooltip("动画过渡时间，0 表示瞬间切换。")]
    // [SerializeField] protected float m_TransitionDuration = 0.1f;
    private int m_AnimationStateHash;
    //私有属性
    private NavMeshAgent _agent;
    private EnemyBase _enemyBase;
    private Vector3 _targetPoint;
    private Vector3 _fixedCenterPoint; // 固定的巡逻中心点，避免每次都计算
    private float _waitTimer;
    private bool _isWaiting;
    private bool _hasTarget;

    private int _retryCount = 0;
    private const int MaxRetryCount = 5; // 最大重试次数


    #region 行为树生命周期方法
    public override void OnAwake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemyBase = GetComponent<EnemyBase>();
    }
    public override void OnStart()
    {
        if (_agent != null)
        {
            _agent.speed = _enemyBase != null ? _enemyBase.EnemyData.walkSpeed : 3.5f; // 设置NavMeshAgent的速度为敌人的移动速度

        }
        _isWaiting = false;
        _hasTarget = false;
        _waitTimer = 0f;
        _retryCount = 0;
        _fixedCenterPoint = patrolCenter != null && patrolCenter.Value != null ? patrolCenter.Value.position : transform.position;
        SetNewTargetPoint();

        // m_AnimationStateHash = Animator.StringToHash(m_AnimationStateName);
        // SetAnimationState(m_AnimationStateHash, m_TransitionDuration);
    }


    public override TaskStatus OnUpdate()
    {
        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogWarning("NavMeshAgent没有正确设置或不在NavMesh上，无法执行巡逻行为。");
            return TaskStatus.Failure;
        }
        if (!_hasTarget)
        {
            SetNewTargetPoint();
            if(!_hasTarget)
            {
                return TaskStatus.Running; // 没有找到有效的目标点，继续找
            }
        }
        if (_isWaiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitTime.Value)
            {
                _isWaiting = false;
                _hasTarget = false;
                SetNewTargetPoint();
            }
            return TaskStatus.Running;
        }
        // 检查是否到达目标点
        if(!_agent.pathPending && _agent.remainingDistance <= arrivalDistance.Value)
        {
            _isWaiting = true;
            _waitTimer = 0f;
            _enemyBase.SetIdle(); // 设置为待机状态
        }

        return TaskStatus.Running;
    }
    public override void OnEnd()
    {
        if (_agent != null && _agent.isOnNavMesh)
            _agent.ResetPath();
        _hasTarget = false;
        _isWaiting = false;
    }
    #endregion
    
    #region 私有方法    
    /// <summary>
    /// 设置新的巡逻目标点
    /// </summary>
    private void SetNewTargetPoint()
    {
        Vector3 center = _fixedCenterPoint;
        //水平平面内随机选取一点
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius.Value;
        Vector3 randomPoint = new Vector3(randomCircle.x, 0, randomCircle.y) + center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 2.0f, NavMesh.AllAreas))
        {
            _targetPoint = hit.position;
            _agent.SetDestination(_targetPoint);
            _enemyBase.SetWalking(); // 设置为行走状态

            _hasTarget = true;
            _retryCount = 0; // 重置重试计数
        }
        else
        {
            _retryCount++;
            _hasTarget = false; // 没有找到有效的点，标记为没有目标
            if (_retryCount >= MaxRetryCount)
            {
                Debug.LogWarning("未能在指定半径内找到有效的巡逻点。");
                //使用中心点作为目标点
                _targetPoint = center;
                _agent.SetDestination(_targetPoint);
                _enemyBase.SetWalking(); // 设置为行走状态
                
                _retryCount = 0; // 重置重试计数
                _hasTarget = true;
                return;     //直接返回，没有继续要做的了
            }

        }
        
    }
    
    // /// <summary>
    // /// 设置动画状态
    // /// </summary>
    // /// <param name="m_AnimationStateHash"></param>
    // /// <param name="m_TransitionDuration"></param>
    // /// <exception cref="System.NotImplementedException"></exception>
    // private void SetAnimationState(int m_AnimationStateHash, float m_TransitionDuration)
    // {
    //     Animator animator = GetComponent<Animator>();
    //     if (animator != null)
    //     {
    //         animator.CrossFadeInFixedTime(m_AnimationStateHash, m_TransitionDuration);
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Animator组件未找到，无法设置动画状态。");
    //     }
    // }
    #endregion
}
