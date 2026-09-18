using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCData
{
    [Header("基础信息")]
    public int npcID;
    public string npcName;
    public GameObject npcPrefab;

    [Header("手动设置的碰撞体相关，用于检测敌人重叠")]
    public float radius;
    public float height;

    [Header("基础属性")]
    [Tooltip("最大生命值")]
    public float maxHealth;
    [Tooltip("当前生命值")]
    public float currentHealth;
    
    [Tooltip("步行速度")]
    public float walkSpeed;
    [Tooltip("奔跑速度")]
    public float runSpeed;

    [Tooltip("旋转速度")]
    public float rotationSpeed;
}

[System.Serializable]
public class EnemyData : NPCData
{
    [Header("攻击判断距离")]
    public float attackDistance;
    [Header("攻击球范围")]
    public float attackSphereRadius;
    [Header("攻击伤害")]
    public float attackDamage;
    [Header("攻击冷却")]
    public float attackCooldown;
    [Header("巡逻范围")]
    public float patrolRadius;
    [Header("是否为守卫")]
    public bool isGuard;

}