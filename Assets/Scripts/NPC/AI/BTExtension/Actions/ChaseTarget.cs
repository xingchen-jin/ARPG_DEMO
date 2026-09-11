using UnityEngine;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine.AI;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
using Opsive.Shared.Utility;


[Category("Custom/AI")]                  
[Description("追逐目标，使用 NavMeshAgent 进行移动。")]
public class ChaseTarget : Action
{
    [Header("追逐目标")]
    public SharedVariable<GameObject> target;

    [Header("默认参数（当 EnemyData 为空时使用）")]
    [SerializeField] private float defaultSpeed = 3.5f;
    [SerializeField] private float defaultStoppingDistance = 1.5f;

    [Header("路径重算阈值")]
    [Tooltip("目标移动超过该距离才重新设置目的地，避免每帧重算路径")]
    [SerializeField] private float repathThreshold = 0.5f;

    //私有属性
    private Transform _targetTransform;
    private NavMeshAgent _agent;
    private EnemyBase _enemyBase;
    //追逐属性
    private float _stoppingDistance;
    private Vector3 _lastTargetPosition;
    private bool _hasSetDestination;

    #region 行为树生命周期方法
    public override void OnAwake()
    {

        _agent = GetComponent<NavMeshAgent>();  
        _enemyBase = GetComponent<EnemyBase>();

        if (_agent == null)
            Debug.LogError($"[ChaseTarget] {gameObject.name} 缺少 NavMeshAgent 组件！");
        if (_enemyBase == null)
            Debug.LogWarning($"[ChaseTarget] {gameObject.name} 未找到 EnemyBase，使用默认参数。");
    }
    public override void OnStart()
    {
        //设置追逐时的属性
        if (_agent != null)
        {
            _agent.speed = _enemyBase != null ? _enemyBase.EnemyData.runSpeed : defaultSpeed; // 设置NavMeshAgent的速度为敌人的移动速度
        }
        _stoppingDistance = _enemyBase != null ? _enemyBase.EnemyData.attackRange : defaultStoppingDistance; // 设置停止距离为敌人的攻击范围

        _agent.isStopped = false; // 确保NavMeshAgent处于移动状态
        _lastTargetPosition = Vector3.positiveInfinity; // 初始化为一个不可能的值，确保第一次会设置目的地
        _hasSetDestination = false;

        _targetTransform = target != null && target.Value != null ? target.Value.transform : null;
    }

    public override TaskStatus OnUpdate()
    {
        // 基础检查
        if (_agent == null || !_agent.isOnNavMesh)
            return TaskStatus.Failure;
        if (target == null || target.Value == null)
            return TaskStatus.Failure;
        if (_targetTransform == null)
            return TaskStatus.Failure;

        // 设置目标位置
        if (!_hasSetDestination || Vector3.Distance(_lastTargetPosition, _targetTransform.position) > repathThreshold)
        {
            if(_agent.SetDestination(_targetTransform.position))
            {
                _lastTargetPosition = _targetTransform.position;
                _enemyBase.SetRunning(); // 设置为奔跑状态
                _hasSetDestination = true;
            }else
            {
                return TaskStatus.Failure;
            }
        }

        // 检查是否到达目标点
        if (!_agent.pathPending && _agent.remainingDistance <= _stoppingDistance)
        {
            // 到达目标点
            return TaskStatus.Success;
        }

        // 继续追逐
        return TaskStatus.Running;
    }
    public override void OnEnd()
    {
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.ResetPath(); // 重置路径，停止追逐
        }
        _hasSetDestination = false;
    }
    #endregion

    #region 私有方法

    #endregion

}
