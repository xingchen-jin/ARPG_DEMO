using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品/枪械数据库管理器。
/// 通过 DefaultExecutionOrder 保证先于其他管理器初始化，
/// 避免其他单例在 Awake 中访问 ItemManager.Instance 时触发空实例竞态。
/// </summary>
[DefaultExecutionOrder(-100)]
public class ItemManager : Singleton<ItemManager>
{
    [SerializeField][Expandable]
    private ItemDatabase_SO itemDatabase;   
    [SerializeField][Expandable]
    private FirearmDetails_SO firearmDatabase;
    private Dictionary<int, ItemBase> baseItemDict = new Dictionary<int, ItemBase>();  // 存储所有物品的详细信息
    private Dictionary<int, FirearmDetails> firearmDict = new Dictionary<int, FirearmDetails>();   // 存储所有枪械的详细信息
    
    protected override void Awake()
    {
        base.Awake();
        BuildDictionaries();
    }

    private void BuildDictionaries()
    {
        baseItemDict.Clear();
        // 构建基础物品字典
        foreach (var item in itemDatabase.ItemDetailsList)
        {
            baseItemDict[item.ItemID] = item;
        }
        firearmDict.Clear();
        // 构建枪械物品字典
        foreach (var firearm in firearmDatabase.FirearmDetailsList)
        {
            if (!baseItemDict.ContainsKey(firearm.itemID))
            {
                Debug.LogWarning($"枪械ID {firearm.itemID} 在基础物品字典中未找到。请确保所有枪械都有对应的基础物品信息。");
                continue;
            }
            firearmDict[firearm.itemID] = firearm;
        }
    }

    /// <summary>
    /// 根据物品ID获取基础物品信息
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <returns></returns>
    public ItemBase GetItemBase(int itemID)
    {
        if (baseItemDict.TryGetValue(itemID, out var itemBase))
        {
            return itemBase;
        }
        Debug.LogWarning($"物品ID {itemID} 在物品字典中未找到。");
        return null;
    }

    /// <summary>
    /// 根据物品ID获取枪械详细信息
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <returns></returns>
    public FirearmDetails GetWeaponDetails(int itemID)
    {
        if (firearmDict.TryGetValue(itemID, out var firearmDetails))
        {
            return firearmDetails;
        }
        Debug.LogWarning($"枪械ID {itemID} 在枪械字典中未找到。");
        return null;
    }
}
