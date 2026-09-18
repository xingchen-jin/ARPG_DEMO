using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人生成点查找器
/// </summary>
public static class SpawnPositionFinder
{
    private static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(1, 0), // 右
        new Vector2Int(-1, 0), // 左
        new Vector2Int(0, 1), // 上
        new Vector2Int(0, -1) // 下
    };

    /// <summary>
    /// 查找有效的生成位置
    /// </summary>
    /// <param name="center">中心位置</param>
    /// <param name="radius">半径</param>
    /// <param name="spawnAreaMask">生成区域掩码</param>
    /// <param name="attemptCount">尝试次数</param>
    /// <param name="maxRadius">最大半径</param>
    /// <param name="validPosition">输出有效位置</param>
    /// <returns></returns>
    // public static bool TryFind(Vector3 center, float radius, LayerMask spawnAreaMask,int attemptCount = 10, int maxRadius, out Vector3 validPosition)
    // {
    //     for(int i = 0; i < attemptCount; i++)
    //     {
    //         //获取中心点
    //     }
    // }

}
