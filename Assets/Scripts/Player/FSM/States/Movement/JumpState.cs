using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class JumpState : IState
{
    private PlayerFSMContext ctx;
    private Vector2 moveInput;

    public JumpState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;

    }
    public void OnEnter()
    {
        ctx.verticalSpeed = ctx.InitVerticalSpeed;
        ctx.animator.SetBool(AnimatorID.IsJumpingID, true);
        ctx.useRootMotion = false; //禁用根运动

        ctx.planarDisplacement = ctx.animator.deltaPosition*ctx.jumpDispMultiplier; //获取动画的位移量

    }

    public void OnExit()
    {
        ctx.verticalSpeed = 0;
        ctx.animator.SetBool(AnimatorID.IsJumpingID, false);
        ctx.useRootMotion = true; //启用根运动
    }

    public void OnFixedUpdate()
    {
    //    ctx.verticalSpeed += ctx.gravity*Time.fixedDeltaTime;
        
       
    }

    public void OnUpdate()
    {

    }
    #region Methods
    #endregion
}
