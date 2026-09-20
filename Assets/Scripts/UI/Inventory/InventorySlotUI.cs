using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("物品图标")]
    [SerializeField] private Image itemIcon;
    [Header("物品数量文本")]
    [SerializeField] private TextMeshProUGUI itemCountText;
    [Header("物品名称")]
    [SerializeField] private TextMeshProUGUI itemNameText;

    //自身按钮
    private Button button;

    public void SetItem(ItemStack itemStack)
    {
        //空处理
        //TODO:后续可以考虑在这里添加一个空物品的图标，或者直接隐藏该物品槽
        if (itemStack == null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
            itemCountText.text = string.Empty;
            itemNameText.text = string.Empty;
            return;
        }
        //图标设置
        ItemBase item = ItemManager.Instance.GetItemBase(itemStack.itemID);
        if (item == null)
        {
            Debug.LogWarning($"物品ID {itemStack.itemID} 在物品字典中未找到。");
            return;
        }
        itemIcon.sprite = item.Icon;
        //数量设置
        itemCountText.text = itemStack.quantity >= 1 ? itemStack.quantity.ToString() : string.Empty;
        //名称设置
        itemNameText.text = item.ItemName;
        
        //TODO:后续通过这个按钮监听对应的点击事件，触发物品使用或装备等操作
    }

}
