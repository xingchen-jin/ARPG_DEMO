using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarPanel : UIBasePanel
{
    #region 子组件名称
    private readonly string HotbarSlot_ArmorName = "HotbarSlot_Armor";
    private readonly string HotbarSlot_FoodName = "HotbarSlot_Food";
    private readonly string HotbarSlot_CureName = "HotbarSlot_Cure";
    #endregion

    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
    #region 组件回调
    /// <summary>
    /// 点击对应快捷槽位事件（目前是打开对应面板）
    /// </summary>
    /// <param name="controlName"></param>
    protected override void ClickBtn(string controlName)
    {
        switch (controlName)
        {
            case "HotbarSlot_Armor":
                Debug.Log("点击了护甲快捷栏");
                UIManager.Instance.ShowPanel<InventoryPanel>(UILevel.Middle, (panel) =>
                {
                    panel.Init(ItemType.Armor);
                });
                break;
            case "HotbarSlot_Food":
                Debug.Log("点击了食物快捷栏");
                UIManager.Instance.ShowPanel<InventoryPanel>(UILevel.Middle, (panel) =>
                {
                    panel.Init(ItemType.Food);
                });
                break;
            case "HotbarSlot_Cure":
                Debug.Log("点击了治疗物品快捷栏");
                UIManager.Instance.ShowPanel<InventoryPanel>(UILevel.Middle, (panel) =>
                {
                    panel.Init(ItemType.Cure);
                });
                break;
            default:
                Debug.LogWarning($"未处理的控件点击事件: {controlName}");
                break;
        }
    }
    #endregion

}
