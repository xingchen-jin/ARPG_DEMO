using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 容器（动态数据）管理器，以后所有的容器的数据层都通过这个类来管理，方便后续扩展和维护
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField]
   private InventoryData inventoryData = new InventoryData();
   public WeaponInstance EquippedWeapon => inventoryData.equippedWeapon;    //从InventoryData中获取当前装备的武器实例
   public int CurrentWeaponAmmo => inventoryData.CurrentWeaponSlotData?.CurrentAmmo ?? 0; //当前装备武器的弹药数量
   public int CurrentWeaponAmmoTotal => inventoryData.CurrentWeaponSlotData?.AmmoTotal ?? 0; //当前装备武器的总弹药数量
   public int EquippedWeaponItemID => inventoryData.equippedWeapon?.itemID ?? 0;
    protected override void Awake()
    {
        base.Awake();
        inventoryData.InitializeWeaponSlots();  // 初始化武器槽数据
    }
    void OnEnable()
    {
        EventCenter.AddListener<SwitchWeaponEvent>(OnSwitchWeaponRequest);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener<SwitchWeaponEvent>(OnSwitchWeaponRequest);
    }
    private void OnSwitchWeaponRequest(SwitchWeaponEvent switchWeaponEvent)
    {
        WeaponType weaponType = switchWeaponEvent.weaponType;
        // 检查是否有该类型的武器槽位,武器
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot.CurrentWeaponInstance == null)return; // 如果没有该类型的武器，直接返回
        
        // 切换当前装备的武器槽位
        inventoryData.SwitchWeaponSlot(weaponType);
        // 触发武器数据变更事件，通知其他系统更新UI或进行其他操作
        if(weaponType == WeaponType.Melee)
        {
            // 如果切换到近战武器，触发武器数据变更事件，传递-1表示没有装备枪械
            EventCenter.EventTrigger<WeaponDataChangedEvent>(new WeaponDataChangedEvent(-1));
            EventCenter.EventTrigger<AmmoDataChangedEvent>(new AmmoDataChangedEvent(-1, -1));
        }else
        {
            EventCenter.EventTrigger<WeaponDataChangedEvent>(new WeaponDataChangedEvent(inventoryData.equippedWeapon.itemID));
            EventCenter.EventTrigger<AmmoDataChangedEvent>(new AmmoDataChangedEvent(inventoryData.CurrentWeaponSlotData?.CurrentAmmo ?? 0, inventoryData.CurrentWeaponSlotData?.AmmoTotal ?? 0));
        }
            
    }
    #region 一般物品管理
    /// <summary>
    /// 添加物品到背包
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="quantity"></param>
    public void AddItemStack(int itemID, int quantity)
    {
        ItemStack existingStack = inventoryData.GetItemStack(itemID);
        if (existingStack != null)
        {
            existingStack.quantity += quantity;
        }
        else
        {
            inventoryData.itemStacks.Add(new ItemStack(itemID, quantity));
        }
    }

    /// <summary>
    /// 从背包中移除物品
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="quantity">数量</param>
    public void RemoveItem(int itemID, int quantity)
    {
        ItemStack existingStack = inventoryData.GetItemStack(itemID);
        if (existingStack != null)
        {
            existingStack.quantity -= quantity;
            if (existingStack.quantity <= 0)
            {
                inventoryData.itemStacks.Remove(existingStack);
            }
        }
    }
    #endregion

    #region 武器管理
    /// <summary>
    /// 获取指定类型的武器ID，如果没有装备该类型的武器，返回-1
    /// </summary>
    /// <param name="weaponType">武器类型</param>
    /// <returns></returns>
    public int GetWeaponID(WeaponType weaponType)
    {
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null && weaponSlot.CurrentWeaponInstance != null)
        {
            return weaponSlot.CurrentWeaponInstance.itemID;
        }
        return -1; // 如果没有装备该类型的武器，返回-1
    }
    /// <summary>
    /// 添加武器，如果武器已存在，则增加弹药数量
    /// </summary>
    /// <param name="weaponID">武器ID</param>
    /// <param name="Ammo">弹药数量</param> 
    public void AddWeapon(int weaponID, int Ammo = 0)
    {
        WeaponType weaponType = ItemManager.Instance.GetWeaponDetails(weaponID).weaponType;
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            weaponSlot.AddWeaponAndAmmo(new WeaponInstance(weaponID), Ammo);
        }else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法添加武器。");
        }
    }
    /// <summary>
    /// 添加武器，如果武器已存在，则增加弹药数量，并使用枪械数据库中的默认弹药数量
    /// </summary>
    /// <param name="weaponID">武器ID</param>
    public void AddWeapon(int weaponID)
    {
        FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(weaponID);
        if (firearmDetails == null)
        {
            Debug.LogWarning($"武器ID {weaponID} 在枪械字典中未找到，无法添加武器。");
            return;
        }

        WeaponType weaponType = firearmDetails.weaponType;
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            weaponSlot.AddWeaponAndAmmo(new WeaponInstance(weaponID), firearmDetails.magazineCapacity);
        }else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法添加武器。");
        }
    }

    /// <summary>
    /// 从背包中移除武器
    /// </summary>
    /// <param name="weaponID">武器ID</param>
    public void RemoveWeapon(int weaponID)
    {
        FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(weaponID);
        if (firearmDetails == null)
        {
            Debug.LogWarning($"武器ID {weaponID} 在枪械字典中未找到，无法移除武器。");
            return;
        }

        WeaponType weaponType = firearmDetails.weaponType;
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            int removedIndex = weaponSlot.RemoveWeapon(new WeaponInstance(weaponID));
            if (removedIndex == -1)
            {
                Debug.LogWarning($"武器ID {weaponID} 不在背包中，无法移除。");
            }
        }else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法移除武器。");
        }
    }
    /// <summary>
    /// 装填弹药给当前装备的武器,并触发AmmoDataChangedEvent事件
    /// </summary>
    public bool ReloadCurrentWeapon()
    {
        InventoryWeaponSlotData currentWeaponSlot = inventoryData.CurrentWeaponSlotData;
        if (currentWeaponSlot != null && currentWeaponSlot.CurrentWeaponInstance != null)
        {
            int ammoNeeded = ItemManager.Instance.GetWeaponDetails(currentWeaponSlot.CurrentWeaponInstance.itemID).magazineCapacity - currentWeaponSlot.CurrentAmmo;
            if (ammoNeeded > 0 && currentWeaponSlot.AmmoTotal > 0)
            {
                int ammoToReload = Mathf.Min(ammoNeeded, currentWeaponSlot.AmmoTotal);
                currentWeaponSlot.AddCurrentAmmo(ammoToReload);
                currentWeaponSlot.RemoveAmmoTotal(ammoToReload);
                EventCenter.EventTrigger<AmmoDataChangedEvent>(new AmmoDataChangedEvent(currentWeaponSlot.CurrentAmmo, currentWeaponSlot.AmmoTotal));
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// 切换当前武器槽位
    /// </summary>
    /// <param name="weaponType"></param>
    public void SwitchWeaponSlot(WeaponType weaponType)
    {
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            inventoryData.currentWeaponType = weaponType;
        }
        else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法切换武器。");
        }
    }

    #endregion

    #region  弹药管理

    /// <summary>
    /// 获取指定类型武器的弹药信息，格式为 "当前弹药/总弹药"，如果没有该类型的武器，返回 "0/0"
    /// </summary>
    /// <param name="weaponType">武器类型</param>
    /// <returns></returns>
    public string GetAmmoInfo(WeaponType weaponType)
    {
        if (weaponType == WeaponType.Melee)
        {
            return string.Empty; // 近战武器没有弹药信息
        }
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null && weaponSlot.CurrentWeaponInstance != null)
        {
            return $"{weaponSlot.CurrentAmmo}/{weaponSlot.AmmoTotal}";
        }
        return string.Empty; // 如果没有该类型的武器，返回默认值
    }
    /// <summary>
    /// 添加弹药到背包中的武器
    /// </summary>
    /// <param name="weaponID"></param>
    /// <param name="ammoAmount"></param>
    public void AddAmmo(int weaponID, int ammoAmount)
    {
        FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(weaponID);
        if (firearmDetails == null)
        {
            Debug.LogWarning($"武器ID {weaponID} 在枪械字典中未找到，无法添加弹药。");
            return;
        }

        WeaponType weaponType = firearmDetails.weaponType;
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            weaponSlot.AddAmmoTotal(ammoAmount);
        }
        else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法添加弹药。");
        }
    }

    /// <summary>
    /// 扣除当前装备武器的弹药
    /// </summary>
    /// <param name="amount"></param>
    public bool DeductCurWeaponAmmo(int amount)
    {
        InventoryWeaponSlotData currentWeaponSlot = inventoryData.CurrentWeaponSlotData;
        if (currentWeaponSlot != null && currentWeaponSlot.CurrentWeaponInstance != null)
        {
            if (currentWeaponSlot.CurrentAmmo >= amount)
            {
                currentWeaponSlot.AddCurrentAmmo(-amount);
                return true;
            }
        }else
        {
            Debug.LogWarning("当前没有装备武器，无法扣除弹药。");
        }
        return false;
    }
    /// <summary>
    /// 检查背包中指定武器的弹药是否足够
    /// </summary>
    /// <param name="weaponID"> 武器ID</param>
    /// <param name="amount"> 需要的弹药数量</param>
    /// <returns></returns>
    public bool checkAmmo(int weaponID, int amount)
    {
        FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(weaponID);
        if (firearmDetails == null)
        {
            Debug.LogWarning($"武器ID {weaponID} 在枪械字典中未找到，无法检查弹药。");
            return false;
        }

        WeaponType weaponType = firearmDetails.weaponType;
        InventoryWeaponSlotData weaponSlot = inventoryData.GetWeaponSlotData(weaponType);
        if (weaponSlot != null)
        {
            return weaponSlot.CurrentAmmo >= amount;
        }
        else
        {
            Debug.LogWarning($"武器类型 {weaponType} 的槽位未初始化，无法检查弹药。");
            return false;
        }
    } 
    /// <summary>
    /// 检查当前装备武器的弹药是否足够
    /// </summary>
    /// <param name="amount">需要的弹药数量</param>
    /// <returns></returns>
    public bool CheckAmmo(int amount)
    {
        InventoryWeaponSlotData currentWeaponSlot = inventoryData.CurrentWeaponSlotData;
        if (currentWeaponSlot != null && currentWeaponSlot.CurrentWeaponInstance != null)
        {
            return currentWeaponSlot.CurrentAmmo >= amount;
        }
        else
        {
            Debug.LogWarning("当前没有装备武器，无法检查弹药。");
            return false;
        }
    }
    #endregion
}
