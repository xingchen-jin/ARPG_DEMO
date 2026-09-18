using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEvent
{

}
#region 武器相关事件
/// <summary>
/// 武器切换事件
/// </summary>
public struct SwitchWeaponEvent : IEvent
{
    public WeaponType weaponType;
    public SwitchWeaponEvent(WeaponType weaponType)
    {
        this.weaponType = weaponType;
    }
}
/// <summary>
/// 武器数据变化事件,id为-1表示武器被卸下
/// </summary>
public struct WeaponDataChangedEvent : IEvent
{
    public int itemID;
    public WeaponDataChangedEvent(int itemID)
    {
        this.itemID = itemID;
    }
}
/// <summary>
/// 弹药数据变化事件，弹药为-1表示没有弹药
/// </summary>
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
/// <summary>
/// 开火请求事件
/// </summary>
public struct FireRequestEvent : IEvent
{
    public int needAmmo;
    public FireRequestEvent(int currentAmmo)
    {
        this.needAmmo = currentAmmo;
    }
}

#endregion

#region UI事件
/// <summary>
/// 切换输入模式事件
/// </summary>
public struct SwitchInputModeEvent : IEvent
{
    public InputMode inputMode;
    public SwitchInputModeEvent(InputMode inputMode)
    {
        this.inputMode = inputMode;
    }
}
/// <summary>
/// 血量更新事件
/// </summary>
public struct UpdateHealthEvent : IEvent
{
    public float curHealth;
    public float maxHealth;
    public UpdateHealthEvent(float curHealth, float maxHealth)
    {
        this.curHealth = curHealth;
        this.maxHealth = maxHealth;
    }
}
#endregion

#region 场景相关事件
/// <summary>
/// 场景加载进度变化事件
/// 传递一个float类型的进度值[0-1]
/// </summary>
public struct SceneLoadChangeEvent : IEvent
{
    public float progress;
    public SceneLoadChangeEvent(float progress)
    {
        this.progress = progress;
    }
}
#endregion