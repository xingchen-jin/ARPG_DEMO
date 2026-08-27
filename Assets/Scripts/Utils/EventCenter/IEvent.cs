using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEvent
{

}
public struct SwitchWeaponEvent : IEvent
{
    public WeaponType weaponType;
    public SwitchWeaponEvent(WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }
}
public struct WeaponDataChangedEvent : IEvent
{
    public int itemID;
    public WeaponDataChangedEvent(int itemID)
    {
        this.itemID = itemID;
    }
}
public struct AmmoDataChangedEvent : IEvent
{
    public int currentAmmo;
    public int totalAmmo;
    public AmmoDataChangedEvent(int currentAmmo, int totalAmmo)
    {
        this.currentAmmo = currentAmmo;
        this.totalAmmo = totalAmmo;
    }
}
public class ReloadRequestEvent : IEvent
{
}
public struct FireRequestEvent : IEvent
{
    public int needAmmo;
    public FireRequestEvent(int currentAmmo)
    {
        this.needAmmo = currentAmmo;
    }
}