using System;
using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponFSM : MonoBehaviour
{
    private FSM<PlayerWeaponState> fsm;
    private PlayerFSMContext ctx;

    public WeaponFSM(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
        fsm = new FSM<PlayerWeaponState>(ctx);

        //添加状态和状态转换条件
        fsm.AddState(PlayerWeaponState.Base, new WeaponBaseState(),ctx);
        fsm.AddState(PlayerWeaponState.Firearm, new WeaponFirearmState(),ctx);
        fsm.AddState(PlayerWeaponState.Aiming, new WeaponAimingState(),ctx);
        fsm.AddState(PlayerWeaponState.Reload, new WeaponReloadState(),ctx);

        fsm.AddTransition(PlayerWeaponState.Base, PlayerWeaponState.Firearm, () => ctx.input.RifleInput);
        fsm.AddTransition(PlayerWeaponState.Firearm, PlayerWeaponState.Base, () => !ctx.input.RifleInput);
        fsm.AddTransition(PlayerWeaponState.Firearm, PlayerWeaponState.Aiming, () => ctx.input.aimInput);
        fsm.AddTransition(PlayerWeaponState.Aiming, PlayerWeaponState.Firearm, () => !ctx.input.aimInput);
        fsm.AddTransition(PlayerWeaponState.Aiming,PlayerWeaponState.Base,()=>!ctx.input.RifleInput);
    }

    public void Start()
    {
        fsm.SwitchState(PlayerWeaponState.Base);
    }
    
    public void OnUpdate()
    {
        Debug.Log($"WeaponFSM当前状态:{fsm.curStateType}");
        //Debug.Log($"firearmInput:{ctx.input.RifleInput},aimInput:{ctx.input.aimInput}");
        fsm.OnUpdate();
    }
    public void OnFixedUpdate()=>fsm.OnFixedUpdate();
    public void OnLateUpdate()=>fsm.OnLateUpdate();
    public bool SwitchState(PlayerWeaponState state)
    {
        if (fsm.curStateType == state)
        {
            return false;
        }
        fsm.SwitchState(state);
        return true;
    }
    #region 动画事件转发处理
    public void OnAnimEvent(string eventName)
    {
        switch (eventName)
        {
            case "WeaponReloadEnd":
                OnAnimEvent_WeaponReloadEnd();
                break;
            default:
                break;
        }
    }

    private void OnAnimEvent_WeaponReloadEnd()
    {
        //处理武器换弹动画结束事件
        //数据处理
        if (!InventoryManager.Instance.ReloadCurrentWeapon())
        {
            Debug.LogWarning("在动画播放完弹药却不够");
        }
        //状态切换
        switch (fsm.prevStateType)
        {
            //TODO:换弹后强制切回基础持枪状态，因为换弹后可能会有瞄准状态的输入，
            // 导致换弹后直接进入瞄准状态，后续可以考虑添加一个换弹后延迟一段时间才能进入瞄准状态的机制
            //且切回瞄准的动画显得不自然，视角有跳跃的感觉
            //瞄准还有IK的小bug
            case PlayerWeaponState.Firearm:
                fsm.SwitchState(PlayerWeaponState.Firearm);
                break;
            case PlayerWeaponState.Aiming:
                fsm.SwitchState(PlayerWeaponState.Firearm);
                break;
            default:
                fsm.SwitchState(PlayerWeaponState.Base);
                Debug.LogWarning("在未设定的状态下播放了换弹动画");
                break;
        }
        
    }
    #endregion
}
