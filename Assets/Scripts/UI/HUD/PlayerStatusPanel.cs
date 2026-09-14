using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusPanel : UIBasePanel
{
    //组件引用
    Slider healthSlider;
    //组件名称
    private static readonly string HealthSliderName = "HPBar";

    public override void HideMe()
    {
   
    }

    public override void ShowMe()
    {
    }
    public void OnEnable()
    {
        //获得自身血条组件
        healthSlider = GetControl<Slider>(HealthSliderName);
        EventCenter.AddListener<UpdateHealthEvent>(OnUpdateHealth);
    }
    public void OnDisable()
    {
        EventCenter.RemoveListener<UpdateHealthEvent>(OnUpdateHealth);
    }
    /// <summary>
    /// 更新血条，根据事件数据中当前血量以及最大血量计算血条的值
    /// </summary>
    /// <param name="eventData"></param>
    private void OnUpdateHealth(UpdateHealthEvent eventData)
    {
        //更新血条
        healthSlider.value = Math.Max(0, eventData.curHealth / eventData.maxHealth);
    }
}
