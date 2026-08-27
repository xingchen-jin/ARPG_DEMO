using System;
using TMPro;
using UnityEngine.UI;

public class WeaponInfoPanel : UIBasePanel
{
    private Image weaponIcon;
    private TextMeshProUGUI ammoCountText;
    public static readonly string nameWeaponIcon = "WeaponIcon";
    public static readonly string nameAmmoCountText = "AmmCountText";

    protected override void Awake()
    {
        base.Awake();
        //获取控件
        weaponIcon = GetControl<Image>(nameWeaponIcon);
        ammoCountText = GetControl<TextMeshProUGUI>(nameAmmoCountText);
    }
    void OnEnable()
    {
        //注册事件
        EventCenter.Instance.AddListener<WeaponInstance>(EventType.WeaponDataChanged, OnWeaponDataChanged);
        EventCenter.Instance.AddListener<int,int>(EventType.AmmoDataChanged, OnAmmoDataChanged);
    }
    void OnDisable()
    {
        //注销事件
        EventCenter.Instance.RemoveListener<WeaponInstance>(EventType.WeaponDataChanged, OnWeaponDataChanged);
        EventCenter.Instance.RemoveListener<int,int>(EventType.AmmoDataChanged, OnAmmoDataChanged);
    }

    private void OnAmmoDataChanged(int ammoCount,int ammoSum)
    {
        //更新UI显示
        ammoCountText.text = ammoCount.ToString() + "/" + ammoSum.ToString();

    }

    private void OnWeaponDataChanged(WeaponInstance weaponInstance)
    {
        //更新UI显示
        UpdateWeaponInfo(weaponInstance);
    }

    private void UpdateWeaponInfo(WeaponInstance weaponInstance)
    {
        int itemID = weaponInstance.itemID;
        // 更新UI显示
        weaponIcon.sprite = ItemManager.Instance.GetItemBase(itemID).Icon;
        //ammoCountText.text = 
    }

    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
}
