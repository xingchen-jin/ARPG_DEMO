using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFSM;
using System;

public class MovementFSM
{
    private FSM<PlayerMovementState> fsm;
    private PlayerFSMContext ctx;

    public MovementFSM(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
        fsm = new FSM<PlayerMovementState>(ctx);
        fsm.AddState(PlayerMovementState.Locomotion, new LocomotionState(ctx));
        fsm.AddState(PlayerMovementState.Jump,new JumpState(ctx));
        fsm.AddState(PlayerMovementState.Crouch,new CrouchingState(ctx));
        fsm.AddState(PlayerMovementState.Climb,new ClimbState(ctx));
        //添加跳跃状态转换条件
        fsm.AddTransition(PlayerMovementState.Locomotion, PlayerMovementState.Jump, () => CanJump());
        fsm.AddTransition(PlayerMovementState.Locomotion, PlayerMovementState.Climb, () => CanClimb());
        fsm.AddTransition(PlayerMovementState.Jump, PlayerMovementState.Locomotion, () => ctx.isGrounded && ctx.verticalSpeed <= 0);
        fsm.AddTransition(PlayerMovementState.Climb, PlayerMovementState.Locomotion, () => ClimbToLocomotion());
        //添加下蹲状态转换条件
        fsm.AddTransition(PlayerMovementState.Locomotion, PlayerMovementState.Crouch, () => ctx.input.crouchInput && ctx.isGrounded);
        fsm.AddTransition(PlayerMovementState.Crouch, PlayerMovementState.Locomotion, () => !ctx.input.crouchInput && ctx.isGrounded);

    }

    public void Start()
    {
        fsm.SwitchState(PlayerMovementState.Locomotion);
    }
    
    public void OnUpdate()
    {
        Debug.Log($"MovementFSM当前状态:{fsm.curStateType}");
        fsm.OnUpdate();
    }

    public void OnFixedUpdate()=>fsm.OnFixedUpdate();
    public void OnLateUpdate()=>fsm.OnLateUpdate();

    
    #region 状态机切换条件
    bool CanJump()
    {
       bool canJump = ctx.input.jumpInput && ctx.isGrounded && !ctx.canClimb;
    //ctx.input.jumpInput = false; //重置跳跃输入，避免重复触发
       return canJump;
    }

    private bool CanClimb()
    {
        return ctx.input.jumpInput && ctx.canClimb;
    }
    private bool ClimbToLocomotion()
    {
        return ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f && ctx.animator.GetCurrentAnimatorStateInfo(0).IsTag("Climb");
    }
    #endregion

    public PlayerMovementState GetCurrentState()
    {
        return fsm.curStateType;
    }
    
}
