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

    #region 武器栏数据
    private Dictionary<WeaponType, InventoryWeaponSlotData> weaponSlots = new Dictionary<WeaponType, InventoryWeaponSlotData>(); //武器槽位数据
    public WeaponType currentWeaponType; //当前武器类型
    public InventoryWeaponSlotData CurrentWeaponSlotData => weaponSlots.ContainsKey(currentWeaponType) ? weaponSlots[currentWeaponType] : null;
    #endregion
    #region 物品容器存储
    /// <summary>
    /// 物品容器存储，不保存武器
    /// </summary>
    private Dictionary<ItemType, List<ItemStack>> itemStacksDictionary = new Dictionary<ItemType, List<ItemStack>>(); //物品容器存储
    public IReadOnlyDictionary<ItemType, List<ItemStack>> ItemStacksDictionary => itemStacksDictionary;

    #endregion
    #region 物品快捷栏数据
    private Dictionary<ItemType,ItemStack> hotbarSlots = new Dictionary<ItemType, ItemStack>(); //快捷栏槽位数据
    #endregion

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
        //初始化容器
        foreach (var stack in itemStacks)
        {
            if (!itemStacksDictionary.ContainsKey(stack.ItemType))
            {
                itemStacksDictionary[stack.ItemType] = new List<ItemStack>();
            }
            itemStacksDictionary[stack.ItemType].Add(stack);
        }

        // 设置快捷栏槽位默认为(列表)第一个物品
        if (HotbarConfig.hotbarItemTypes.Length > 0)
        {
            foreach (var itemType in HotbarConfig.hotbarItemTypes)
            {
                if (itemStacksDictionary.ContainsKey(itemType) && itemStacksDictionary[itemType].Count > 0)
                {
                    hotbarSlots[itemType] = itemStacksDictionary[itemType][0];
                }
            }
        }
    }
    #region 一般物品数据
    /// <summary>
    /// 获取物品堆叠（会在静态数据中查找类型）
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <returns></returns>
    public ItemStack GetItemStack(int itemID)
    {
        ItemType itemType = ItemManager.Instance.GetItemBase(itemID)?.ItemType ?? ItemType.None;
        if (itemStacksDictionary.ContainsKey(itemType))
        {
            return itemStacksDictionary[itemType].Find(stack => stack.itemID == itemID);
        }
        return null;
    }

    /// <summary>
    /// 获取物品堆叠(指定类型)
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="itemType">物品类型</param>
    /// <returns></returns>
    public ItemStack GetItemStack(int itemID,ItemType itemType)
    {
        if (itemStacksDictionary.ContainsKey(itemType))
        {
            return itemStacksDictionary[itemType].Find(stack => stack.itemID == itemID);
        }
        return null;
    }
    /// <summary>
    /// 获取指定类型的物品堆叠列表
    /// </summary>
    /// <param name="itemType">物品类型</param>
    /// <returns></returns>
    public List<ItemStack> GetItemStacksByType(ItemType itemType)
    {
        if (itemStacksDictionary.ContainsKey(itemType))
        {
            return itemStacksDictionary[itemType];
        }
        return new List<ItemStack>();
    }
    public void AddItem(int itemID, int quantity)
    {
        ItemType itemType = ItemManager.Instance.GetItemBase(itemID)?.ItemType ?? ItemType.None;
        if (itemType == ItemType.None)
        {
            Debug.LogWarning($"尝试添加物品ID {itemID}，但未找到对应的物品类型。");
            return;
        }

        if (!itemStacksDictionary.ContainsKey(itemType))
        {
            itemStacksDictionary[itemType] = new List<ItemStack>();
        }

        var existingStack = itemStacksDictionary[itemType].Find(stack => stack.itemID == itemID);
        if (existingStack != null)
        {
            existingStack.quantity += quantity;
        }
        else
        {
            var newStack = new ItemStack(itemID, quantity);
            itemStacksDictionary[itemType].Add(newStack);
            // 在有新的物品添加时，同步到总列表
            itemStacks.Add(newStack); 
        }
    }
    /// <summary>
    /// 减少指定数量的物品（当所需要删除的不足是拒绝并返回false）
    /// </summary>
    /// <param name="itemID">物品ID</param>
    /// <param name="quantity">数量</param>
    /// <returns></returns>
    public bool RemoveItem(int itemID, int quantity)
    {
        ItemType itemType = ItemManager.Instance.GetItemBase(itemID)?.ItemType ?? ItemType.None;
        if (itemType == ItemType.None)
        {
            Debug.LogWarning($"尝试移除物品ID {itemID}，但未找到对应的物品类型。");
            return false;
        }

        if (!itemStacksDictionary.ContainsKey(itemType))
        {
            return false;
        }

        var existingStack = itemStacksDictionary[itemType].Find(stack => stack.itemID == itemID);
        if (existingStack != null && existingStack.quantity >= quantity)
        {
            existingStack.quantity -= quantity;
            if (existingStack.quantity <= 0)
            {
                itemStacksDictionary[itemType].Remove(existingStack);
                // 同步到总列表
                itemStacks.Remove(existingStack); 
            }
            return true;
        }
        return false;
    }
    
    #endregion
    #region 武器数据
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
    #endregion
}