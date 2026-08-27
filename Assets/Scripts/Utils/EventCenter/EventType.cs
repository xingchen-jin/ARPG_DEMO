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
    /// 切换武器（枪）事件,数据层完成切换后发布请求,<WeaponType>
    /// </summary>
    WeaponDataChanged,
    /// <summary>
    /// 弹药数据改变事件,<int,int>  当前弹药数,总弹药数
    /// </summary>
    AmmoDataChanged, 
    /// <summary>
    /// 换弹事件,
    /// </summary> 
    ReloadRequest,  
    /// <summary>
    /// 开火事件    <int>子弹数
    /// </summary>
    FireRequest,   

}
