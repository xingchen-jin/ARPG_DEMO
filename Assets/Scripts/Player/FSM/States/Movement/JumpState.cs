using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class JumpState : StateBase
{
    private PlayerFSMContext ctx;
    private Vector2 moveInput;

    public JumpState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;

    }
    public override void OnEnter()
    {
        ctx.input.jumpInput = false; //重置跳跃输入，避免重复触发

        ctx.verticalSpeed = ctx.InitVerticalSpeed;
        ctx.animator.SetBool(AnimatorID.IsJumpingID, true);
        ctx.useRootMotion = false; //禁用根运动

        ctx.planarDisplacement = ctx.animator.deltaPosition*ctx.jumpDispMultiplier; //获取动画的位移量

    }

    public override void OnExit()
    {
        // ctx.verticalSpeed = 0;
        ctx.animator.SetBool(AnimatorID.IsJumpingID, false);
        ctx.useRootMotion = true; //启用根运动
    }

    #region Methods
    #endregion
}
