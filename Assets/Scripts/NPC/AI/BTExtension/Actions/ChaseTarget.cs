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

    [Header("停止距离")]
    [Tooltip("停止距离 = 攻击范围 × 该系数。留出一段滞回距离，避免刚好停在攻击范围边界上导致行为分支来回切换。")]
    [SerializeField, Range(0.1f, 1f)] private float stoppingDistanceScale = 0.8f;

    //私有属性
    private Transform _targetTransform;
    private NavMeshAgent _agent;
    private EnemyBase _enemyBase;
    private NPCPerception _npcPerception;
    //追逐属性
    private float _stoppingDistance;
    private Vector3 _lastTargetPosition;
    private bool _hasSetDestination;

    #region 行为树生命周期方法
    public override void OnAwake()
    {

        _agent = GetComponent<NavMeshAgent>();
        _enemyBase = GetComponent<EnemyBase>();
        _npcPerception = GetComponent<NPCPerception>();

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
            _agent.speed = GetRunSpeed(); // 设置NavMeshAgent的速度为敌人的移动速度
        }
        _stoppingDistance = GetStoppingDistance(); // 停在攻击范围之内，而不是停在边界上

        _agent.isStopped = false; // 确保NavMeshAgent处于移动状态
        _lastTargetPosition = Vector3.positiveInfinity; // 初始化为一个不可能的值，确保第一次会设置目的地
        _hasSetDestination = false;

        _targetTransform = ResolveTargetTransform();
    }

    public override TaskStatus OnUpdate()
    {
        // 基础检查
        if (_agent == null || !_agent.isOnNavMesh)
            return TaskStatus.Failure;

        //目标可能在追逐途中被替换或销毁，每帧重新取一次
        _targetTransform = ResolveTargetTransform();
        if (_targetTransform == null)
            return TaskStatus.Failure;

        // 设置目标位置
        if (!_hasSetDestination || Vector3.Distance(_lastTargetPosition, _targetTransform.position) > repathThreshold)
        {
            if(_agent.SetDestination(_targetTransform.position))
            {
                _lastTargetPosition = _targetTransform.position;
                if (_enemyBase != null) _enemyBase.SetRunning(); // 设置为奔跑状态
                _hasSetDestination = true;
            }else
            {
                //寻路暂时失败（例如目标点不在 NavMesh 上）时保持 Running 下一帧重试；
                //返回 Failure 会让上层选择器立刻切到巡逻分支，来回切换会造成抖动。
                _hasSetDestination = false;
                return TaskStatus.Running;
            }
        }

        // // 检查是否到达目标点
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
    /// <summary>
    /// 取当前要追逐的目标：
    /// 优先使用行为树共享变量，变量未绑定或目标已销毁时退回到 NPCPerception 检测到的玩家，
    /// 保证即使图中变量没连好也不会出现“看得到却追不上去”的情况。
    /// </summary>
    private Transform ResolveTargetTransform()
    {
        if (target != null && target.Value != null)
        {
            return target.Value.transform;
        }
        if (_npcPerception != null && _npcPerception.DetectedPlayer != null)
        {
            return _npcPerception.DetectedPlayer;
        }
        return null;
    }

    /// <summary>
    /// 获取奔跑速度
    /// </summary>
    private float GetRunSpeed()
    {
        if (_enemyBase != null && _enemyBase.EnemyData != null)
        {
            return _enemyBase.EnemyData.runSpeed;
        }
        return defaultSpeed;
    }

    /// <summary>
    /// 获取停止距离：略小于攻击范围，避免贴着判定边界停下
    /// </summary>
    private float GetStoppingDistance()
    {
        if (_enemyBase != null && _enemyBase.EnemyData != null)
        {
            return _enemyBase.EnemyData.attackDistance * stoppingDistanceScale;
        }
        return defaultStoppingDistance;
    }
    #endregion

}
