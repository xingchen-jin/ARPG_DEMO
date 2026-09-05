using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenuPanel : UIBasePanel
{
    public override bool BlocksPlayerInput => true;
    public readonly string PistolBtnName = "PistolBtn";
    public readonly string RifleBtnName = "RifleBtn";
    public readonly string ShotgunBtnName = "ShotgunBtn";
    public readonly string SniperBtnName = "SniperBtn";
    public readonly string RocketLauncherBtnName = "RocketLauncherBtn";
    public readonly string SubmachineGunBtnName = "SubmachineGunBtn";
    public readonly string GrenadeBtnName = "GrenadeBtn";
    public readonly string MeleeBtnName = "MeleeBtn";
    private Dictionary<string, WeaponType> buttonWeaponTypeMap = new Dictionary<string, WeaponType>()
    {
        {"PistolBtn", WeaponType.Pistol},
        {"RifleBtn", WeaponType.Rifle},
        {"ShotgunBtn", WeaponType.Shotgun},
        {"SniperBtn", WeaponType.Sniper},
        {"RocketLauncherBtn", WeaponType.RocketLauncher},
        {"SubmachineGunBtn", WeaponType.SubmachineGun},
        {"GrenadeBtn", WeaponType.Grenade},
        {"MeleeBtn", WeaponType.Melee}

    };

    public override void HideMe()
    {
        this.gameObject.SetActive(false);
        CameraManager.Instance.HideCursorAndLock();
        EventCenter.EventTrigger<SwitchInputModeEvent>(new SwitchInputModeEvent(InputMode.Gameplay));
    }

    public override void ShowMe()
    {
        this.gameObject.SetActive(true);
        ShowAllWeaponInfo();
        CameraManager.Instance.ShowCursorAndUnlock();
        EventCenter.EventTrigger<SwitchInputModeEvent>(new SwitchInputModeEvent(InputMode.UI));
    }
    /// <summary>
    /// 显示所有按钮的武器信息
    /// </summary>
    private void ShowAllWeaponInfo()
    {
        foreach (var button in buttonWeaponTypeMap)
        {
            //获取按钮的武器类型
            WeaponType weaponType = button.Value;
            //获取按钮的名称
            string btnName = button.Key;
            //获取按钮的控件
            var btn = GetControl<Button>(btnName);
            //获取按钮的图标控件
            var icon = btn.transform.Find("Icon").GetComponent<UnityEngine.UI.Image>();
            //获取按钮的弹药数量文本控件
            var ammoCountText = btn.transform.Find("Ammo").GetComponent<TMPro.TextMeshProUGUI>();
            //根据武器类型获取武器相关信息
            int ID = InventoryManager.Instance.GetWeaponID(weaponType);
            string ammoInfo = InventoryManager.Instance.GetAmmoInfo(weaponType);
            //获取武器图标
            var weaponDetails = ItemManager.Instance.GetItemBase(ID);
            if (weaponDetails != null)
            {
                icon.sprite = weaponDetails.Icon;
                icon.enabled = true;
            }
            else
            {
                icon.enabled = false;
            }
            ammoCountText.text = ammoInfo;
        }

    }
    /// <summary>
    /// 点击按钮时调用的函数
    /// </summary>
    /// <param name="btnName"></param>
    protected override void ClickBtn(string btnName)
    {
        WeaponType weaponType = WeaponType.Melee;
        switch (btnName)
        {
            case "PistolBtn":
                weaponType = WeaponType.Pistol;
                break;
            case "RifleBtn":
                weaponType = WeaponType.Rifle;
                break;
            case "ShotgunBtn":
                weaponType = WeaponType.Shotgun;
                break;
            case "SniperBtn":
                weaponType = WeaponType.Sniper;
                break;
            case "RocketLauncherBtn":
                weaponType = WeaponType.RocketLauncher;
                break;
            case "SubmachineGunBtn":
                weaponType = WeaponType.SubmachineGun;
                break;
            case "GrenadeBtn":
                weaponType = WeaponType.Grenade;
                break;
            case "MeleeBtn":
                weaponType = WeaponType.Melee;
                break;
            default:
                weaponType = WeaponType.Melee;
                break;
        }
        //发送事件
       EventCenter.EventTrigger<SwitchWeaponEvent>(new SwitchWeaponEvent(weaponType));
       HideMe();
       UIManager.Instance.HidePanel<RadialMenuPanel>();
    }

}
