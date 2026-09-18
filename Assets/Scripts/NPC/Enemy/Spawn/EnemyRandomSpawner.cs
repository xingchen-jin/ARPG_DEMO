using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct EnemySpawnData
{
    public int enemyID; // 敌人ID
    public int spawnCount; // 生成数量
}

/// <summary>
/// 敌人生成器
/// </summary>
public class EnemyRandomSpawner : MonoBehaviour
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
    [Tooltip("获取随机点次数限制")]
    [SerializeField]private float randomAttemptLimit = 10f; 

    [Header("Debug")]
    [Tooltip("是否显示生成范围")]
    [SerializeField]private bool showSpawnArea = true; // 是否显示生成范围
    [Tooltip("生成范围分段数")]
    [SerializeField]private int SpawnSpawnSegments = 50; // 生成范围分段数
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
            float radius = enemyData.radius; // 获取敌人重叠检测半径
            float height = enemyData.height; // 获取敌人重叠检测高度
            //开始生成敌人
            //TODO: 随机获取生成点位置
            //TODO：检测生成是否合法
                //向下射线是否为地面或可生成区域（Layer）
                //该范围是否在NavMesh上（NavMesh.SamplePosition）
                //该区域是否有其他敌人（Physics.OverlapSphere）
            for (int i = 0; i < enemySpawnData.spawnCount; i++)
            {
                //获取随机的生成位置并检测合法性
                //若不合法进行
                if (TrySpawnEnemy(height, radius))
                {
                    
                }
                
            }

        }
    }
    private bool TrySpawnEnemy(float height,float radius = 0.5f)
    {
        for(int i=0;i<randomAttemptLimit;i++)
        {
            //获取随机生成点位置
            Vector3 spawnPosition = GetRandomSpawnPosition();
            //获取该位置的地面位置
            if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hitInfo, 20f, spawnAreaMask))
            {
                //获取地面位置
                Vector3 groundPosition = hitInfo.point;
                    //检测该位置是否在NavMesh上
                    if (NavMesh.SamplePosition(groundPosition, out NavMeshHit navMeshHit, 1f, NavMesh.AllAreas))
                    {
                    //检测该位置是否有其他敌人
                    if (!HasOverlapEnemy(navMeshHit.position, height, radius))
                    {
                        // 生成敌人
                        SpawnEnemy(NPCManager.Instance.GetNpcData<EnemyData>(enemyList[0].enemyID), navMeshHit.position);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 检测该位置是否有其他敌人(胶囊体)
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="radius">半径</param>
    /// <param name="height">高度</param>
    /// <returns></returns>
    private bool HasOverlapEnemy(Vector3 position, float height,float radius = 0.5f)
    {
        Collider[] colliders = Physics.OverlapCapsule(position + Vector3.up * radius,position+ Vector3.up * (height+radius), radius);
        return colliders.Length > 0;
    }
    /// <summary>
    /// 获取随机生成点位置
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPosition = spawnAreaCenter + new Vector3(UnityEngine.Random.Range(-spawnAreaRadius, spawnAreaRadius), 0, UnityEngine.Random.Range(-spawnAreaRadius, spawnAreaRadius));
        return randomPosition;
    }

    private void SpawnEnemy(EnemyData enemyData, Vector3 position)
    {
        //实例化敌人
        //TODO: 这里可以使用对象池来优化性能
        //TODO:生成和检测之后可以加入法线方向适配复杂地形。
        GameObject enemy = Instantiate(enemyData.npcPrefab, position, Quaternion.identity);
        enemy.transform.SetParent(transform, false); // 将敌人设置为生成器的子对象，方便管理
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
        DrawFlatCircle(spawnAreaCenter, spawnAreaRadius, spawnAreaColor, SpawnSpawnSegments);
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
