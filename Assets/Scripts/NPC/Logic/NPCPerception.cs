using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPerception : MonoBehaviour
{
    [Header("视野参数")]
    [Space(10)]

    [Header("视野距离")]
    [SerializeField] private float viewDistance = 20f; // 视野距离
    [Header("视野角度")]
    [SerializeField] private float viewAngle = 120f; // 视野角，角度制
    [Header("障碍物层")]
    [SerializeField] private LayerMask obstacleMask; // 障碍物层
    [Header("玩家层")]
    [SerializeField] private LayerMask PlayerMask; // 玩家层
    [Header("眼睛位置")]
    [SerializeField] private Transform eyeTransform; // 眼睛位置
    [Space(10)]

    [Header("调试参数")]
    [SerializeField] private bool showDebug = true; // 是否显示调试信息

    //当前检测到的玩家
    private Transform detectedPlayer;
    public Transform DetectedPlayer => detectedPlayer;
    
    #region Unity生命周期方法
    void Update()
    {

        DetectPlayer();
        Debug.Log($"当前检测到的玩家: {detectedPlayer?.gameObject.name ?? "无"}");
    }
    #endregion


    #region 私有方法
    /// <summary>
    /// 检测玩家是否在NPC的视野范围内，只要检测到一个玩家就返回
    /// </summary>
    private void DetectPlayer()
    {
        //Transform npcTransform = transform; // 获取NPC自身的Transform组件
        detectedPlayer = null; // 重置检测到的玩家

        // 使用球形初步筛选
        Collider[] playersInViewRadius = Physics.OverlapSphere(eyeTransform.position, viewDistance, PlayerMask);
        foreach (Collider hitCollider in playersInViewRadius)
        {
          // Debug.Log($"检测到物体: {hitCollider.gameObject.name}，尝试进行视野判断。");
            Transform target = hitCollider.transform;

            //计算方向与角度
            Vector3 directionToTarget = (target.position - eyeTransform.position).normalized;
            float angle = Vector3.Angle(eyeTransform.forward, directionToTarget);

            //视锥角度判断
            if (angle > viewAngle * 0.5f) continue; // 不在视野内

            //距离判断（可选，因为 OverlapSphere 已经过滤了距离，但为了精确可再算一次）
            float distance = Vector3.Distance(eyeTransform.position, target.position);
            if (distance > viewDistance) continue;

            //视线遮挡检测（射线检测）
            Vector3 targetPoint = target.position + Vector3.up * 0.5f; 
            
            RaycastHit hit;
            if (Physics.Linecast(eyeTransform.position, targetPoint, out hit, obstacleMask))
            {
                // 如果射线碰到了障碍物，说明视线被遮挡
                if (hit.collider.gameObject != target.gameObject)
                {
                    continue; // 被遮挡，忽略该玩家
                }
            }

            //通过所有检测，记录并返回
            detectedPlayer = target;
            //Debug.Log($"NPC {gameObject.name} 检测到玩家: {detectedPlayer.gameObject.name}");
            return; 
        }
    }
    #endregion

    #region 公共方法
    public Transform GetDetectedPlayer()
    {
        return detectedPlayer;
    }
    #endregion

    #region 调试方法
    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        // 绘制视野范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyeTransform.position, viewDistance);

        // 绘制视野角度
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * eyeTransform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * eyeTransform.forward;
        Gizmos.DrawLine(eyeTransform.position, eyeTransform.position + leftBoundary * viewDistance);
        Gizmos.DrawLine(eyeTransform.position, eyeTransform.position + rightBoundary * viewDistance);
        //绘制扇形
        int segments = 20; // 扇形分段数
        for (int i = 0; i <= segments; i++)
        {
            float angle = -viewAngle / 2 + (viewAngle * i / segments);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * eyeTransform.forward;
            Gizmos.DrawLine(eyeTransform.position, eyeTransform.position + direction * viewDistance);
        }

        // 绘制检测到的玩家
        if (detectedPlayer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyeTransform.position, detectedPlayer.position+ Vector3.up * 0.5f);
        }
    }
    #endregion
}
