using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XC_Framework;

[System.Serializable]
public class InventoryData
{
    public List<ItemStack> itemStacks = new List<ItemStack>();          //物品列表
    [SerializeField]
    private List<WeaponInstance> weaponInstances = new List<WeaponInstance>(); //武器实例列表
    [SerializeField]
    private List<Pair<WeaponType, int>> weaponSlotAmmo = new List<Pair<WeaponType, int>>(); //武器弹药

    private Dictionary<WeaponType, InventoryWeaponSlotData> weaponSlots = new Dictionary<WeaponType, InventoryWeaponSlotData>(); //武器槽位数据
    public WeaponType currentWeaponType; //当前武器类型
    public InventoryWeaponSlotData CurrentWeaponSlotData => weaponSlots.ContainsKey(currentWeaponType) ? weaponSlots[currentWeaponType] : null;

    public WeaponInstance equippedWeapon => CurrentWeaponSlotData?.CurrentWeaponInstance;
    public void InitializeWeaponSlots()
    {
        foreach (WeaponType weaponType in System.Enum.GetValues(typeof(WeaponType)))
        {
            weaponSlots[weaponType] = new InventoryWeaponSlotData();
        }
        // 初始化武器槽位弹药数据
        foreach (var pair in weaponSlotAmmo)
        {
            if (weaponSlots.ContainsKey(pair.first))
            {
                weaponSlots[pair.first].AddAmmoTotal(pair.second);
            }
        }
        // 将武器实例添加到对应的槽位中
        foreach (var weaponInstance in weaponInstances)
        {
            FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(weaponInstance.itemID);
            if (firearmDetails == null)
            {
                Debug.LogWarning($"武器ID {weaponInstance.itemID} 在枪械字典中未找到，初始化槽位时跳过。");
                continue;
            }
            WeaponType weaponType = firearmDetails.weaponType;
            if (weaponSlots.ContainsKey(weaponType))
            {
                // Pair 为 struct，用 FindIndex 判断是否找到弹药配置
                int ammoIndex = weaponSlotAmmo.FindIndex(pair => pair.first == weaponType);
                int ammo = ammoIndex >= 0 ? weaponSlotAmmo[ammoIndex].second : 0;
                weaponSlots[weaponType].AddWeapon(weaponInstance);
            }
        }

    }

    /// <summary>
    /// 获取物品堆叠
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <returns></returns>
    public ItemStack GetItemStack(int itemID)
    {
        return itemStacks.Find(stack => stack.itemID == itemID);
    }
    
    /// <summary>
    /// 获取武器实例
    /// </summary>
    /// <param name="weaponType">武器类型</param>
    /// <returns></returns>
    public InventoryWeaponSlotData GetWeaponSlotData(WeaponType weaponType)
    {
        return weaponSlots.GetValueOrDefault(weaponType);
    }
    /// <summary>
    /// 切换武器槽位
    /// </summary>
    /// <param name="weaponType">武器类型</param>
    public void SwitchWeaponSlot(WeaponType weaponType)
    {
        if (weaponSlots.ContainsKey(weaponType))
        {
            currentWeaponType = weaponType;
        }
        else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法切换。");
        }
    }

}
