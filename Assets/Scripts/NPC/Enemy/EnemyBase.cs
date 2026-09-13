using System;
using System.Collections;
using System.Collections.Generic;
using Opsive.BehaviorDesigner.Runtime;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BehaviorTree))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private int npcID; // 敌人的唯一ID，用于从数据库中获取数据

    [Header("是否在场景中手动添加")]
    [SerializeField] private bool isSceneOriginObj = false; // 是否在场景中手动添加敌人数据

    //敌人的基础属性接口
    public EnemyData EnemyData => enemyData;
    //自身组件
    private Animator anim;
    private BehaviorTree behaviorTree;
    //动画播放机的动画哈希值
    private static readonly int MoveSpeedID = Animator.StringToHash("MoveSpeed");

    //动画参数阈值
    [SerializeField]private float idleThreshold = 0;
    [SerializeField]private float walkThreshold = 1.5f;
    [SerializeField]private float RunThreshold = 3.5f;

    //动画其他参数
    [Tooltip("动画参数平滑时间，越小越快")]
    [SerializeField]private float MoveDampTime = 0.1f;

    //动画移动参数的目标值与平滑后的当前值
    private float _targetMoveSpeed;
    private float _currentMoveSpeed;
    private float _moveSpeedVelocity;

    [Header("攻击判定球")]
    [SerializeField]private Transform attackPoint; // 攻击点位置
    [SerializeField]private LayerMask attackMask; // 攻击层

    [Header("调试")]
    [Tooltip("是否显示攻击范围")]
    [SerializeField]private bool showAttackDistance = false; // 是否显示攻击范围
    [SerializeField]private Color attackDistanceColor = Color.red; // 攻击范围颜色
    [Tooltip("是否显示攻击球半径")]
    [SerializeField]private bool showAttackSphereRadius = false; // 是否显示攻击球半径
    [SerializeField]private Color attackSphereRadiusColor = Color.blue; // 攻击球半径颜色



    #region 生命周期
    void Awake()
    {
        anim = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();

        //将敌人数据传递给行为树
        SyncAttackDistanceToBehaviorTree();
    }
    void Update()
    {
        if (anim == null) return;
        //Animator.SetFloat 的阻尼重载需要每帧调用才会收敛，
        //只在状态切换时调用一次，参数只会移动一小段距离，混合树永远到不了行走/奔跑阈值。
        //因此这里统一在 Update 中把参数平滑推向目标值。
        _currentMoveSpeed = Mathf.SmoothDamp(_currentMoveSpeed, _targetMoveSpeed, ref _moveSpeedVelocity, MoveDampTime);
        anim.SetFloat(MoveSpeedID, _currentMoveSpeed);
    }
    void Start()
    {
        if(!isSceneOriginObj)return; // 如果是手动添加的敌人数据，则不从数据库中获取数据

        Init(); // 初始化敌人数据
        isSceneOriginObj = false; // 标记为非场景天然存在的敌人，可以在OnEnable中重新获取数据
    }
    void OnEnable()
    {
        if(isSceneOriginObj)return; // 如果是场景天然存在的敌人，则不从数据库中获取数据

        Init(); 
    }

    #endregion


    #region 碰撞检测
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 处理与玩家的碰撞逻辑
            Debug.Log("敌人碰到了玩家！");
        }
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // 处理与子弹的碰撞逻辑
            Debug.Log("敌人被子弹击中！");
            // 可以在这里调用敌人的受伤方法，减少生命值等
        }
    }
    #endregion
    
    #region 公有方法
    #region  一般方法
    /// <summary>
    /// 受到伤害的方法，实现IDamageable接口
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="attacker"></param>
    public void TakeDamage(float damage, GameObject attacker)
    {
        // 处理敌人受伤逻辑
        Debug.Log($"敌人受到 {damage} 点伤害！");
        enemyData.currentHealth -= damage; // 减少当前生命值
        if (enemyData.currentHealth <= 0)
        {
            Die();
        }
        // 可以在这里减少敌人的生命值，播放受伤动画等

    }
    #endregion
    #region 动画设置

    /// <summary>
    /// 设置动画移动参数的目标值，具体数值由 Update 平滑写入 Animator。
    /// 行为树节点只需声明“想以多快的速度移动”，不需要每帧调用。
    /// </summary>
    /// <param name="speed">目标移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        _targetMoveSpeed = speed;
    }
    /// <summary>
    /// 设置为待机
    /// </summary>
    public void SetIdle()
    {
        SetMoveSpeed(idleThreshold);
    }
    /// <summary>
    /// 设置为行走
    /// </summary>
    public void SetWalking()
    {
        SetMoveSpeed(walkThreshold);
    }
    /// <summary>
    /// 设置为奔跑
    /// </summary>
    public void SetRunning()
    {
        SetMoveSpeed(RunThreshold);
    }
    #endregion
    #region 动画事件
    public void OnAttackHitEvent()
    {
        TrrigerAttack();
    }
    #endregion
    #endregion

    #region 私有方法
    private void Init()
    {
        //初始化敌人数据
        enemyData = NPCManager.Instance.GetNpcData<EnemyData>(npcID);
        if (enemyData == null)
        {
            Debug.LogError($"未能获取到敌人数据，敌人ID: {npcID}，请检查数据库！");
            return; // 取不到数据时不要继续，否则下面会空引用
        }
        enemyData.currentHealth = enemyData.maxHealth; // 初始化当前生命值

        //数据是从数据库重新取的，行为树里的攻击范围要同步刷新，
        //否则行为树还在用 Awake 时写入的预制体数值，和 ChaseTarget 使用的攻击范围不一致。
        SyncAttackDistanceToBehaviorTree();
    }

    /// <summary>
    /// 触发攻击
    /// </summary>
    private void TrrigerAttack()
    {
        //在攻击点位置创建一个球形碰撞体，检测所有在攻击范围内的敌人
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, enemyData.attackSphereRadius, attackMask);
        foreach (var hitCollider in hitColliders)
        {
            //检查碰撞体是否有IDamageable接口
            IDamageable damageable = hitCollider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                //调用受伤方法，传入伤害值和攻击者对象
                damageable.TakeDamage(enemyData.attackDamage, gameObject);
            }
        }
    }
    #region 行为树设置
    /// <summary>
    /// 把当前的攻击范围同步给行为树，保证行为树判断和移动逻辑用的是同一个值
    /// </summary>    
    private void SyncAttackDistanceToBehaviorTree()
    {
        if (behaviorTree != null && enemyData != null)
        {
            behaviorTree.SetVariableValue("AttackDistance", enemyData.attackDistance);
        }
    }
    #endregion
    private void Die()
    {
        // 处理敌人死亡逻辑
        Debug.Log("敌人死亡！");
        // 可以在这里播放死亡动画，掉落物品等
        Destroy(gameObject); // 销毁敌人对象
    }

    #endregion

    #region Debug方法
    private Mesh _rangeMesh; // 用于绘制攻击范围的Mesh
    private void OnDrawGizmosSelected()
    {
        if(enemyData == null) return;
        if(showAttackDistance)
        {
            //脚底位置
            Vector3 position = transform.position + Vector3.up * 0.1f; // 提升一点高度，避免与地面重叠
            float radius = enemyData.attackDistance; // 使用敌人的攻击范围
            DrawFlatCircle(position, radius, attackDistanceColor, 64);
        }
        if(showAttackSphereRadius && attackPoint != null)
        {
            Gizmos.color = attackSphereRadiusColor;
            Gizmos.DrawWireSphere(attackPoint.position, enemyData.attackSphereRadius);
        }
    }


    private void DrawFlatCircle(Vector3 center, float radius,Color color,int segments = 64)
    {
        //更改画笔元素
        Color preColor = Gizmos.color;
        Gizmos.color = color;
        //获得步长
        float step = 2f * Mathf.PI / segments;
        //计算起始点
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        //类似微分的思想画圆
        for (int i = 1; i <= segments; i++)
        {
            float angle = step * i;
            Vector3 currentPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }

        //恢复画笔颜色
        Gizmos.color = preColor;
    }
private Mesh GetRangeMesh()
{
    if (_rangeMesh != null) return _rangeMesh;

    const int segments = 64;
    _rangeMesh = new Mesh { name = "AttackRangeDisc" };
    _rangeMesh.hideFlags = HideFlags.HideAndDontSave;

    var vertices = new Vector3[segments + 1];
    var triangles = new int[segments * 3];

    vertices[0] = Vector3.zero;                    // 圆心
    for (int i = 0; i < segments; i++)
    {
        float angle = 2f * Mathf.PI * i / segments;
        vertices[i + 1] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    for (int i = 0; i < segments; i++)
    {
        int t = i * 3;
        triangles[t]     = 0;
        triangles[t + 1] = (i + 1) % segments + 1;  // 改为逆时针
        triangles[t + 2] = i + 1;
    }

    _rangeMesh.vertices = vertices;
    _rangeMesh.triangles = triangles;
    _rangeMesh.RecalculateNormals();
    _rangeMesh.RecalculateBounds();
    return _rangeMesh;
}
    #endregion
}
