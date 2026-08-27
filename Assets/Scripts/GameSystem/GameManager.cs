using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        //初始化UI
        UIManager.Instance.ShowPanel<CrosshairPanel>(UILevel.Middle);
        UIManager.Instance.ShowPanel<WeaponInfoPanel>(UILevel.Middle);
        UIManager.Instance.ShowPanel<PlayerStatusPanel>(UILevel.Middle);
        
    }
}
