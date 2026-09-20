using UnityEngine;
public enum ItemType
{
    None,   //无
    Firearm, //枪械
    Ammo,    //弹药
    Armor,  //护甲
    Food,   //食物
    Cure,  //治疗物品
    QuestItem   //任务物品
}

//全局唯一的物品信息
[System.Serializable]
public class ItemBase
{
    public int itemID;  //全局唯一的物品ID
    [SerializeField]private string itemName;
    [SerializeField]private ItemType itemType;
    [SerializeField]private Sprite icon;
    [SerializeField]private GameObject prefab;
    [SerializeField]private string description;

    public int ItemID => itemID;
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public Sprite Icon => icon;
    public GameObject Prefab => prefab;
    public string Description => description;
}
public static class ItemTypeHelper
{
    public static string GetItemTypeName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Ammo:
                return "弹药";
            case ItemType.Firearm:
                return "枪械";
            case ItemType.Armor:
                return "护甲";
            case ItemType.Food:
                return "食物";
            case ItemType.Cure:
                return "药品";
            case ItemType.QuestItem:
                return "任务物品";
            case ItemType.None:
                return "无类型";
            default:
                return "未知类型";
        }
    }
}

#region 具体物品类型的详细信息类
[System.Serializable]
public class FirearmDetails
{
    public int itemID;  //全局唯一的物品ID
    public WeaponType weaponType;   //武器类型
    public int magazineCapacity;  //默认弹匣容量
    public float damage;    //伤害
    public float fireRate;  //射速
    public float range;     //射程
    public Transform leftHandIKTarget;  //左手IK目标
    public Transform firePoint;  //开火点
    
    
}   
[System.Serializable]
public class AmmoDetails
{
    public int itemID;  //全局唯一的物品ID
    public WeaponType weaponType;   //武器类型
}
#endregion