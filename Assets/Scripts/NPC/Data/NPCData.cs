using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCData
{
    public int npcID;
    public string npcName;
    public GameObject npcPrefab;
    public float maxHealth;
    public float currentHealth;
    public float moveSpeed;
    public float rotationSpeed;
}

[System.Serializable]
public class EnemyData : NPCData
{
    [Header("攻击范围")]
    public float attackRange;
    [Header("攻击伤害")]
    public float attackDamage;
    [Header("攻击冷却")]
    public float attackCooldown;
    [Header("检测范围")]
    public float detectionRange;
    [Header("巡逻范围")]
    public float patrolRange;
    [Header("是否为守卫")]
    public bool isGuard;
}