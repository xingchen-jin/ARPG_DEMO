using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 背包面板
/// </summary>
public class InventoryPanel : UIBasePanel
{
    [Tooltip("物品类型")]
    private ItemType itemType;
    [Tooltip("物品数据")]
    private List<ItemStack> items;
    private GameObject slotPrefab;
    #region 子组件
    private TextMeshProUGUI heading;
    private Transform content;
    #endregion
    private readonly string slotPrefabPath = "UI/Btn/InventorySlot";

    #region 子组件名称
    private readonly string HeadingName = "Heading";
    private readonly string ScrollRectName = "InventoryScrollRect"; 
    private readonly string  ViewportName = "InventoryViewport";
    #endregion
    private void Awake()
    {
        base.Awake();
        //同步的方式获取数据
        slotPrefab =ResManager.Instance.Load<GameObject>(slotPrefabPath);
        //获取ScrollRect组件
        ScrollRect rect = GetControl<ScrollRect>(ScrollRectName);
        content = rect.content;
        //获取标题
        heading = GetControl<TextMeshProUGUI>(HeadingName);
    }
    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
    
    /// <summary>
    /// 初始化，传入物品类型
    /// </summary>
    public void Init(ItemType type)
    {
        //设置标题
        heading.text = string.Format("{0}背包", ItemTypeHelper.GetItemTypeName(type));
        //获取物品数据
        items = InventoryManager.Instance.GetAllItemStacks(type);
        //清空现有的物品槽
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        //创建新的物品槽
        foreach (var itemStack in items)
        {
            GameObject slotObj = Instantiate(slotPrefab, content);
            InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
            //设置物品图标和数量
            slotUI.SetItem(itemStack);
        }

    }
}
