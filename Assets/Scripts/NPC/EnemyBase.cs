using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private int npcID; // 敌人的唯一ID，用于从数据库中获取数据

    [Header("是否在场景中手动添加")]
    [SerializeField] private bool isSceneOriginObj = false; // 是否在场景中手动添加敌人数据

    #region 生命周期
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
}
