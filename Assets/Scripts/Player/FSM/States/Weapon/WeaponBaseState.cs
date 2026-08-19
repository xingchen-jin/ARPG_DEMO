using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponBaseState : IState
{
    private PlayerFSMContext ctx;

    public WeaponBaseState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;

    }
    public void OnEnter()
    {
        ctx.animator.SetBool(AnimatorID.IsFirearmID, false);
        ctx.animator.SetBool(AnimatorID.IsAimingID, false);
        
        ctx.leftHandIK.weight = 0;
        ctx.rightHandIK.weight = 0;

        ctx.canRoate = true;
        ctx.weapon.SetActive(false);
    }

    public void OnExit()
    {
    }

    public void OnFixedUpdate()
    {
   
    }

    public void OnUpdate()
    {
    }
}
