using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HotbarConfig
{
    //快捷栏物品类型
    public static readonly ItemType[] hotbarItemTypes = new ItemType[] { 
        ItemType.Armor,  //护甲
        ItemType.Cure,   //治疗物品
        ItemType.Food,   //食物
    }; 
    public static readonly int hotbarSize = hotbarItemTypes.Length; //快捷栏大小
    /// <summary>
    /// 判断物品类型是否是快捷栏物品类型
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns></returns>
    public static bool IsHotbarItemType(ItemType itemType)
    {
        foreach (var type in hotbarItemTypes)
        {
            if (type == itemType)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 判断快捷栏索引是否有效
    /// </summary>
    /// <param name="index">快捷栏索引</param>
    /// <returns></returns>
    public static bool IsValidHotbarIndex(int index)
    {
        return index >= 0 && index < hotbarSize;
    }
}
