using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemStack
{
    public int itemID;
    public int quantity;
    public ItemType ItemType => ItemManager.Instance.GetItemBase(itemID)?.ItemType ?? ItemType.None;
    public ItemStack(int itemID, int quantity)
    {
        this.itemID = itemID;
        this.quantity = quantity;
    }
}

[System.Serializable]
public class WeaponInstance
{
    public int itemID;
    // public int currentAmmo;

    public WeaponInstance(int itemID)
    {
        this.itemID = itemID;
        // this.currentAmmo = currentAmmo;
    }
}


