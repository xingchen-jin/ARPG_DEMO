using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    [SerializeField] private NPC_Database_SO npcDatabase;
    private Dictionary<int,NPCData> npcDict = new Dictionary<int, NPCData>();
    #region 生命周期函数
    protected override void Awake()
    {
        base.Awake();
        InitializeNPCDictionary();
    }
    #endregion

    #region 公有方法
    
    /// <summary>
    /// 根据NPC ID获取对应的NPC数据
    /// </summary>
    /// <typeparam name="T">NPC数据类型</typeparam>
    /// <param name="npcID">npc ID</param>
    /// <returns></returns>
    public T GetNpcData<T>(int npcID) where T : NPCData
    {
        if(npcDict.TryGetValue(npcID, out NPCData npcData))
        {
            return npcData as T;
        }
        else
        {
            Debug.LogError($"在{npcDatabase.name}中未找到NPC ID: {npcID}，请检查数据库！");
            return null;
        }
    }
    #endregion

    #region 私有方法
    private void InitializeNPCDictionary()
    {
        foreach(var enemyData in npcDatabase.enemies)
        {
            if(!npcDict.ContainsKey(enemyData.npcID))
            {
                npcDict.Add(enemyData.npcID, enemyData);
            }
            else
            {
                Debug.LogWarning($"在{npcDatabase.name}中发现重复的NPC ID: {enemyData.npcID}，请检查数据库！");
            }
        }

    }
    #endregion
}
