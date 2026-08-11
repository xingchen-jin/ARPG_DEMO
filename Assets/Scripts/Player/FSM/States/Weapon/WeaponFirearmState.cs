using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponFirearmState : IState
{
    private PlayerFSMContext ctx;

    public WeaponFirearmState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }

    public void OnEnter()
    {
       ctx.animator.SetBool("isFirearm", true);
       ctx.animator.SetBool("isAiming", false);
       
        ctx.leftHandIK.weight = 1;
        ctx.rightHandIK.weight = 1;

        ctx.canRoate = true;
        ctx.input.aimInput = false;//TODO：硬编写，防止提前输入导致一下进入瞄准，后续可能可以添加输入管理器
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
