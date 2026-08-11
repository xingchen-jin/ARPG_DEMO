using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponAimingState : IState
{
    private PlayerFSMContext ctx;

    public WeaponAimingState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }

    public void OnEnter()
    {
       ctx.animator.SetBool("isFirearm", false);
       ctx.animator.SetBool("isAiming", true);
       
        ctx.leftHandIK.weight = 1;
        ctx.rightHandIK.weight = 1;

       ctx.canRun = false;
       ctx.canRoate = false;
    }

    public void OnExit()
    {
        ctx.animator.SetBool("isAiming", false);
        ctx.canRun = true;
        ctx.canRoate = true;
    }

    public void OnFixedUpdate()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
