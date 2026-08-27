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
        EventCenter.Instance.AddListener<WeaponDataChangedEvent>(OnWeaponDataChanged);  
        EventCenter.Instance.AddListener<AmmoDataChangedEvent>(OnAmmoDataChanged);
    }
    void OnDisable()
    {
        //注销事件
        EventCenter.Instance.RemoveListener<WeaponDataChangedEvent>(OnWeaponDataChanged);
        EventCenter.Instance.RemoveListener<AmmoDataChangedEvent>(OnAmmoDataChanged);
    }

    private void OnAmmoDataChanged(AmmoDataChangedEvent eventData)
    {
        int ammoCount = eventData.currentAmmo;
        int ammoSum = eventData.totalAmmo;
        //更新UI显示
        ammoCountText.text = ammoCount.ToString() + "/" + ammoSum.ToString();

    }

    private void OnWeaponDataChanged(WeaponDataChangedEvent eventData)
    {
        //更新UI显示
        UpdateWeaponInfo(eventData.itemID);
    }

    private void UpdateWeaponInfo(int itemID)
    {
        // 更新UI显示
        weaponIcon.sprite = ItemManager.Instance.GetItemBase(itemID).Icon;
    }

    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
}
