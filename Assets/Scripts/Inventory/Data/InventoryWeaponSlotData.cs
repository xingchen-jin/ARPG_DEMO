using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryWeaponSlotData
{
    [SerializeField]
    private List<WeaponInstance> _weapons = new List<WeaponInstance>();    //武器实例列表
    private int _currentIndex = -1; //当前槽位武器索引
    private int _ammoTotal = 0; //当前总弹药数量
    private int _currentAmmo = 0; //当前弹药数量
    public int CurrentAmmo => _currentAmmo;
    public int AmmoTotal => _ammoTotal;
    public WeaponInstance CurrentWeaponInstance => (_currentIndex >= 0 && _currentIndex < _weapons.Count) ? _weapons[_currentIndex] : null;
    public string GetDisplayText => _weapons.Count > 0 ? $"{_currentIndex + 1}/{_weapons.Count}" : string.Empty;
    
    /// <summary>
    /// 添加武器以及指定的弹药数量，如果武器已存在，则增加弹药数量
    /// </summary>
    /// <param name="weaponInstance">武器</param>
    /// <param name="Ammo">指定的弹药数量</param>
    public void AddWeaponAndAmmo(WeaponInstance weaponInstance,int Ammo)
    {
        _ammoTotal += Ammo;
        if (!_weapons.Contains(weaponInstance))
        {
            _weapons.Add(weaponInstance);
            if (_currentIndex == -1)
            {
                SwtichWeapon(0); //如果当前没有武器，则切换到新添加的武器
            }
        }
    }

    /// <summary>
    /// 添加武器，如果武器已存在，则不做任何操作
    /// </summary>
    /// <param name="weaponInstance"></param>
    public void AddWeapon(WeaponInstance weaponInstance)
    {
        if (!_weapons.Contains(weaponInstance))
        {
            _weapons.Add(weaponInstance);
            if (_currentIndex == -1)
            {
                SwtichWeapon(0); //如果当前没有武器，则切换到新添加的武器
            }
        }else
        {
            Debug.LogWarning($"武器 {weaponInstance.itemID} 已存在于槽位中。");
        }
    }
    /// <summary>
    /// 移除武器，如果当前武器被移除，则切换到下一把武器
    /// </summary>
    /// <param name="weaponInstance"></param>
    /// <returns></returns>
    public int RemoveWeapon(WeaponInstance weaponInstance)
    {
        int index = _weapons.IndexOf(weaponInstance);
        if (index != -1)
        {
            _weapons.RemoveAt(index);
            if (_currentIndex >= _weapons.Count)
            {
                SwtichWeapon(_weapons.Count - 1); //调整当前索引
            }
        }
        return index;
    }
    
    /// <summary>
    /// 切换武器，并自动补充弹药
    /// </summary>
    /// <param name="index"></param>
    private void SwtichWeapon(int index)
    {
        if (index >= 0 && index < _weapons.Count)
        {
            //切换武器时，更新当前弹药数量
            _ammoTotal += _currentAmmo;
            _currentAmmo = Math.Min(_ammoTotal, ItemManager.Instance.GetWeaponDetails(_weapons[index].itemID).magazineCapacity);//更新当前弹药数量为新武器的弹药数量
            _ammoTotal -= _currentAmmo; //从总弹药中扣除当前弹
            _currentIndex = index;
        }
    }
    /// <summary>
    /// 切换到下一把武器
    /// </summary>
    public void NextWeapon()
    {
        if (_weapons.Count > 0)
        {
            SwtichWeapon((_currentIndex + 1) % _weapons.Count);
        }
    }
    /// <summary>
    /// 切换到上一把武器
    /// </summary>
    public void PreviousWeapon()
    {
        if (_weapons.Count > 0)
        {
            SwtichWeapon((_currentIndex - 1 + _weapons.Count) % _weapons.Count);
        }
    }
    #region 弹药管理W

    /// <summary>
    /// 增加弹药数量
    /// </summary>
    /// <param name="amount">增加的弹药数量</param>
    public void AddAmmoTotal(int amount)
    {
        _ammoTotal += amount;
    }
    /// <summary>
    /// 减少总弹药数量
    /// </summary>
    /// <param name="amount">减少的弹药数量</param>
    public void RemoveAmmoTotal(int amount)
    {
        _ammoTotal -= amount;
    }
    public void AddCurrentAmmo(int amount)
    {
        _currentAmmo += amount;
    }
    /// <summary>
    /// 减少当前武器的弹药数量
    /// </summary>
    /// <param name="amount">减少的弹药数量</param>
    /// <returns></returns>
    public bool RemoveCurrentAmmo(int amount)
    {
        if (_currentAmmo >= amount)
        {
            _currentAmmo -= amount;
            return true;
        }
        return false;
    }


    #endregion
    #region 一般方法
    public bool empty => _weapons.Count == 0;
    public int Count => _weapons.Count;
    #endregion
}
