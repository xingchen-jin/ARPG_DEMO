using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyFSM;

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
        //添加跳跃状态转换条件
        fsm.AddTransition(PlayerMovementState.Locomotion, PlayerMovementState.Jump, () => CanJump());
        fsm.AddTransition(PlayerMovementState.Jump, PlayerMovementState.Locomotion, () => ctx.isGrounded && ctx.verticalSpeed <= 0);
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

    #region 状态机切换条件
    bool CanJump()
    {
       bool canJump = ctx.input.jumpInput && ctx.isGrounded;
       ctx.input.jumpInput = false; //重置跳跃输入，避免重复触发

       return canJump;
    }
    #endregion

    public PlayerMovementState GetCurrentState()
    {
        return fsm.curStateType;
    }
    
}
