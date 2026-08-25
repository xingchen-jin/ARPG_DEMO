using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponFirearmState : StateBase
{
    private PlayerFSMContext ctx;

    public WeaponFirearmState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }

    public override void OnEnter()
    {
       ctx.animator.SetBool(AnimatorID.IsFirearmID, true);
       ctx.animator.SetBool(AnimatorID.IsAimingID, false);
       
        ctx.weaponController.SetLeftHandIKWeight(1);
        //ctx.rightHandIK.weight = 1;
        ctx.rightHandIK.weight = 0;

        ctx.canRoate = true;
        ctx.input.aimInput = false;//TODO：硬编写，防止提前输入导致一下进入瞄准，后续可能可以添加输入管理器
    
        ctx.weaponController.EnableWeapon(true);
    }

}
