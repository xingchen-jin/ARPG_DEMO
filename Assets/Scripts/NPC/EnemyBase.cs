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
    [SerializeField]private float MoveDampTime = 0.1f;

    #region 生命周期
    void Awake()
    {
        anim = GetComponent<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();

        if(behaviorTree != null && enemyData != null)
        {
            //将敌人数据传递给行为树
            behaviorTree.SetVariableValue("AttackRange", enemyData.attackRange);
        }
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
    /// <summary>
    /// 受到伤害的方法，实现IDamageable接口
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="attacker"></param>
    public void TakeDamage(int damage, GameObject attacker)
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
    #region 动画设置

    /// <summary>
    /// 直接设置动画移动参数
    /// </summary>
    /// <param name="speed">移动速度</param>
    public void SetMoveSpeed(float speed)
    {
        //设置动画混合树速度
        anim.SetFloat(MoveSpeedID,speed,0.1f,Time.deltaTime);  
    }
    /// <summary>
    /// 设置为行走
    /// </summary>
    public void SetIdle()
    {
        Debug.Log($"设置为待机状态，速度阈值: {idleThreshold}");
        anim.SetFloat(MoveSpeedID,idleThreshold,MoveDampTime,Time.deltaTime);
    }
    public void SetWalking()
    {
        Debug.Log($"设置为行走状态，速度阈值: {walkThreshold}");
        anim.SetFloat(MoveSpeedID,walkThreshold,MoveDampTime,Time.deltaTime);
    }
    public void SetRunning()
    {
        anim.SetFloat(MoveSpeedID,RunThreshold,MoveDampTime,Time.deltaTime);
    }
    #endregion
    #region 动画事件
    public void OnAttackHitEvent()
    {
        //TODO:设置攻击范围
        //TODO:获取命中对象
        //TODO:执行伤害逻辑
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
        }
        enemyData.currentHealth = enemyData.maxHealth; // 初始化当前生命值
    }
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
        //脚底位置
        Vector3 position = transform.position + Vector3.up * 0.1f; // 提升一点高度，避免与地面重叠
        float radius = enemyData.attackRange; // 使用敌人的攻击范围

        //半透明实心圆
        Gizmos.color = new Color(1f, 0f, 0f, 1f); // 红色，半透明
        Gizmos.DrawMesh(GetRangeMesh(), position, Quaternion.identity,new Vector3(radius, 1f, radius)); // 绘制圆形Mesh，缩放为直径
        DrawFlatCircle(position, radius, Color.red, 64);
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
