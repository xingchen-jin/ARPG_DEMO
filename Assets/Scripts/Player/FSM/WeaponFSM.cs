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
        fsm.AddState(PlayerWeaponState.Base, new WeaponBaseState(ctx));
        fsm.AddState(PlayerWeaponState.Firearm, new WeaponFirearmState(ctx));
        fsm.AddState(PlayerWeaponState.Aiming, new WeaponAimingState(ctx));

        fsm.AddTransition(PlayerWeaponState.Base, PlayerWeaponState.Firearm, () => ctx.input.RifleInput);
        fsm.AddTransition(PlayerWeaponState.Firearm, PlayerWeaponState.Base, () => !ctx.input.RifleInput);
        fsm.AddTransition(PlayerWeaponState.Firearm, PlayerWeaponState.Aiming, () => ctx.input.aimInput);
        fsm.AddTransition(PlayerWeaponState.Aiming, PlayerWeaponState.Firearm, () => !ctx.input.aimInput);

    }

    public void Start()
    {
        fsm.SwitchState(PlayerWeaponState.Base);
    }
    
    public void OnUpdate()
    {
        Debug.Log($"WeaponFSM当前状态:{fsm.curStateType}");
        fsm.OnUpdate();
    }
    public void OnFixedUpdate()=>fsm.OnFixedUpdate();
}
