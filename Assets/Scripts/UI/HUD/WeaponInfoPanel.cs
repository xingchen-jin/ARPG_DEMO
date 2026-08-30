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
        EventCenter.AddListener<WeaponDataChangedEvent>(OnWeaponDataChanged);  
        EventCenter.AddListener<AmmoDataChangedEvent>(OnAmmoDataChanged);
    }
    void OnDisable()
    {
        //注销事件
        EventCenter.RemoveListener<WeaponDataChangedEvent>(OnWeaponDataChanged);
        EventCenter.RemoveListener<AmmoDataChangedEvent>(OnAmmoDataChanged);
    }

    private void OnAmmoDataChanged(AmmoDataChangedEvent eventData)
    {
        if (eventData.currentAmmo < 0 || eventData.totalAmmo < 0)
        {
            //如果弹药为-1，表示没有弹药，清空UI显示
            ammoCountText.text = String.Empty;
            return;
        }
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
        if (itemID < 0)
        {
            //如果itemID为-1，表示武器被卸下，清空UI显示
            weaponIcon.enabled = false;
            return;
        }
        weaponIcon.enabled = true;
        // 更新UI显示
        weaponIcon.sprite = ItemManager.Instance.GetItemBase(itemID).Icon;
    }

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {
        //显示UI时，清空武器图标和弹药数量显示
        weaponIcon.enabled = false;
        ammoCountText.text = String.Empty;
    }
}
