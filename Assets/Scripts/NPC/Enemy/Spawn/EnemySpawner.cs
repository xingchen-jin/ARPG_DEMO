using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EnemySpawnData
{
    public int enemyID; // 敌人ID
    public int spawnCount; // 生成数量

}

/// <summary>
/// 敌人生成器
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("敌人生成器数据")]
    [SerializeField]private List<EnemySpawnData> enemyList; 

    [Header("生成范围(原形范围),从上往下射线检测地面")]
    [Tooltip("生成范围中心点偏移")]
    [SerializeField]private Vector3 spawnAreaCenterOffset; // 生成范围中心点偏移
    [Tooltip("生成范围检测层")]
    [SerializeField]private LayerMask spawnAreaMask; // 生成范围检测层
    [Tooltip("生成范围半径")]
    [SerializeField]private float spawnAreaRadius; // 生成范围半径

    [Header("生成点检测")]
    [Tooltip("生成点检测次数限制")]
    [SerializeField]private float checkLimit = 10f; // 检测限制

    [Header("Debug")]
    [Tooltip("是否显示生成范围")]
    [SerializeField]private bool showSpawnArea = true; // 是否显示生成范围
    [Tooltip("生成范围分段数")]
    [SerializeField]private float SpawnSpawnSegments = 50; // 生成范围分段数
    [Tooltip("生成范围颜色")]
    [SerializeField]private Color spawnAreaColor = Color.red; // 生成范围颜色
    [Tooltip("生成点颜色")]
    [SerializeField]private Color spawnPointColor = Color.green; // 生成点颜色
    [Tooltip("生成点半径")]
    [SerializeField]private float spawnPointRadius = 0.01f; // 生成点半径

    private Vector3 spawnAreaCenter; // 生成范围中心点

    #region 生命周期
    void Awake()
    {
        //计算生成范围中心点
        spawnAreaCenter = transform.position + spawnAreaCenterOffset;
    }

    void OnEnable()
    {
        //生成敌人
        SpawnEnemies();
    }
    
    #endregion

    #region 公共方法
    #endregion

    #region 私有方法
    private void SpawnEnemies()
    {
        foreach (var enemySpawnData in enemyList)
        {
            //根据敌人ID获取敌人数据
            EnemyData enemyData = NPCManager.Instance.GetNpcData<EnemyData>(enemySpawnData.enemyID);
            //开始生成敌人
            //TODO: 随机获取生成点位置
            //TODO：检测生成是否合法
                //向下射线是否为地面或可生成区域（Layer）
                //该范围是否在NavMesh上（NavMesh.SamplePosition）
                //该区域是否有其他敌人（Physics.OverlapSphere）
            for (int i = 0; i < enemySpawnData.spawnCount; i++)
            {
                //尝试生成敌人，限制检测次数
                for (int j = 0; j < checkLimit; j++)
                {
                    //随机获取生成点位置
                    Vector3 spawnPosition = GetRandomSpawnPosition();
                    //检测生成是否合法
                    if (IsValidSpawnPosition(spawnPosition, enemyData))
                    {
                        //生成敌人
                        Instantiate(enemyData.npcPrefab, spawnPosition, Quaternion.identity);
                        break; // 生成成功，跳出检测循环
                    }
                    else
                    {
                        Debug.LogWarning($"敌人生成点不合法，敌人ID: {enemySpawnData.enemyID}，请检查生成范围！");
                    }
                }
            }

        }
    }
    /// <summary>
    /// 检测生成点是否合法
    /// </summary>
    /// <param name="spawnPosition">生成点位置</param>
    /// <param name="enemyData">生成的敌人数据</param>
    /// <returns></returns>
    private bool IsValidSpawnPosition(Vector3 spawnPosition,EnemyData enemyData)
    {
        return true; // TODO: 实现生成点合法性检测
    }
    /// <summary>
    /// 获取随机生成点位置
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomSpawnPosition()
    {
        return Vector3.zero; // TODO: 实现随机获取生成点位置
    }

    #endregion

    #region Debug
    /// <summary>
    /// 绘制生成范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 绘制生成范围
        if (!showSpawnArea) return;

        //计算生成范围中心点(在编辑器中实时计算，为了在编辑器中移动生成器时，生成范围也能跟随移动)
        Vector3 spawnAreaCenter = transform.position + spawnAreaCenterOffset;
        //绘制中心点
        Gizmos.color = spawnPointColor;
        Gizmos.DrawSphere(spawnAreaCenter, spawnPointRadius);

        //绘制原形
        DrawFlatCircle(spawnAreaCenter, spawnAreaRadius, spawnAreaColor, 50);
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
    #endregion

}
