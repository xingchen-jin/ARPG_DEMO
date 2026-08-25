using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    /// <summary>
    /// 切换武器（枪）事件,输入层发布请求
    /// </summary>
    SwitchWeaponRequest,
    /// <summary>
    /// 切换武器（枪）事件,数据层完成切换后发布请求
    /// </summary>
    WeaponDataChanged,
    /// <summary>
    /// 换弹事件
    /// </summary> 
    ReloadWeapon,   //换弹事件
    /// <summary>
    /// 开火事件
    /// </summary>
    FireWeapon,     //开火事件
}
