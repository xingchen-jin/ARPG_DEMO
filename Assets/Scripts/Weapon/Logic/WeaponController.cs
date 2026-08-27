using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponController : MonoBehaviour
{
    #region 组件引用
    [Header("组件引用")]
    [SerializeField] private Transform weaponParent; //武器挂载点
    [SerializeField] private TwoBoneIKConstraint leftHandIK; //左手IK约束

    public Transform LeftHandIKTarget => leftHandIK != null ? leftHandIK.data.target : null;
    public Transform FirePoint => currentWeaponBehavior != null ? currentWeaponBehavior.firePoint : null;
    
    private GameObject currentWeaponModel; //当前武器 
    private WeaponBehavior currentWeaponBehavior; //当前武器行为脚本
    #endregion
    void OnEnable()
    {
        EventCenter.Instance.AddListener<WeaponDataChangedEvent>(OnSwitchWeapon);
    }
    void OnDisable()
    {
        EventCenter.Instance.RemoveListener<WeaponDataChangedEvent>(OnSwitchWeapon);
    }

    private void OnSwitchWeapon(WeaponDataChangedEvent weaponDataChangedEvent)
    {
        int itemID = weaponDataChangedEvent.itemID;
        if (itemID <= 0)
        {
            Debug.LogWarning("武器ID异常传入");
            return;
        }

        EquipWeapon(itemID);
    }
    /// <summary>
    /// 装备武器
    /// </summary>
    /// <param name="weaponPrefab">武器预制件</param>
    private void EquipWeapon(int itemID)
    {
        // 销毁当前武器模型
        UnequipWeapon();
        // 根据itemID加载武器预制件
        ItemBase itemBase = ItemManager.Instance.GetItemBase(itemID);
        FirearmDetails firearmDetails = ItemManager.Instance.GetWeaponDetails(itemID);
        if (itemBase != null && firearmDetails != null && itemBase.ItemType == ItemType.Firearm)
        {
            GameObject weaponPrefab = itemBase.Prefab;
            if (weaponPrefab != null)
            {
                // 实例化武器模型并挂载到武器挂载点
                currentWeaponModel = Instantiate(weaponPrefab, weaponParent);
                currentWeaponBehavior = currentWeaponModel.GetComponent<WeaponBehavior>();

                // 设置开火点和左手IK目标点（武器预制体运行时重新引用）
                if (currentWeaponBehavior != null && currentWeaponBehavior.LeftHandIKTarget != null)
                {
                    leftHandIK.data.target = currentWeaponBehavior.LeftHandIKTarget;
                }
                else
                {
                    // 武器预制体未配置IK目标时，禁用左手IK避免解算失效目标导致锁死
                    Debug.LogWarning($"武器预制体 {weaponPrefab.name} 未配置LeftHandIKTarget，左手IK已禁用。");
                    leftHandIK.data.target = null;
                    leftHandIK.weight = 0;
                }
            }
            else
            {
                Debug.LogWarning($"武器静态数据错误：物品ID {itemID}无法找到对应的武器预制件。请检查物品数据库中的Prefab字段。");
            }
        }
        else
        {
            Debug.LogWarning($"未找到ID为 {itemID} 的枪械详细信息。");
        }
    }
    /// <summary>
    /// 卸下当前武器
    /// </summary>
    private void UnequipWeapon()
    {
        // 销毁当前武器模型
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
            currentWeaponModel = null;
            currentWeaponBehavior = null;
        }
        // 卸下武器后重置左手IK目标与权重，避免IK引用无效/失效对象导致骨骼锁死
        if (leftHandIK != null)
        {
            leftHandIK.data.target = null;
            leftHandIK.weight = 0;
        }
    }
    /// <summary>
    /// 设置左手IK权重
    /// </summary>
    /// <param name="weight"></param>
    public void SetLeftHandIKWeight(float weight)
    {
        if (leftHandIK != null)
        {
            leftHandIK.weight = weight;
        }
    }
    /// <summary>
    /// 启用或禁用武器模型
    /// </summary>
    /// <param name="enable"></param>
    public void EnableWeapon(bool enable)
    {
        if (currentWeaponModel != null)
        {
            currentWeaponModel.SetActive(enable);
        }
    }
}
