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
        ctx.animator.SetBool("isFirearm", false);
        ctx.animator.SetBool("isAiming", false);
        
        ctx.leftHandIK.weight = 0;
        ctx.rightHandIK.weight = 0;

        ctx.canRoate = true;
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
