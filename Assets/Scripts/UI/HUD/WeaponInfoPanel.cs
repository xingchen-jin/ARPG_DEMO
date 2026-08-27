using TMPro;
using UnityEngine.UI;

public class WeaponInfoPanel : UIBasePanel
{
    private Image weaponIcon;
    private TextMeshProUGUI ammoCountText;
    
    protected override void Awake()
    {
        base.Awake();
        //获取控件
        weaponIcon = GetControl<Image>("WeaponIcon");
        ammoCountText = GetControl<TextMeshProUGUI>("AmmoCountText");
    }
    void OnEnable()
    {
        //注册事件
        EventCenter.Instance.AddListener<WeaponInstance>(EventType.WeaponDataChanged, OnWeaponDataChanged);
    }
    void OnDisable()
    {
        //注销事件
        EventCenter.Instance.RemoveListener<WeaponInstance>(EventType.WeaponDataChanged, OnWeaponDataChanged);
    }

    private void OnWeaponDataChanged(WeaponInstance weaponInstance)
    {
        //更新UI显示
        UpdateWeaponInfo(weaponInstance);
    }

    private void UpdateWeaponInfo(WeaponInstance weaponInstance)
    {
        
    }

    public override void HideMe()
    {
        
    }

    public override void ShowMe()
    {
        
    }
}
