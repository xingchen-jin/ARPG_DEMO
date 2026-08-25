using UnityEngine;
public enum ItemType
{
    Firearm, //枪械
    Ammo,    //弹药
    Armor,  //护甲
    Consumable, //消耗品
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