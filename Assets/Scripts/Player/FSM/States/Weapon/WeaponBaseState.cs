using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponBaseState : StateBase
{
    private PlayerFSMContext ctx;

    public WeaponBaseState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;

    }
    public override void OnEnter()
    {
        ctx.animator.SetBool(AnimatorID.IsFirearmID, false);
        ctx.animator.SetBool(AnimatorID.IsAimingID, false);
        
        ctx.weaponController.SetLeftHandIKWeight(0);
        ctx.rightHandIK.weight = 0;

        ctx.canRoate = true;
        ctx.weaponController.EnableWeapon(false);
    }

}
