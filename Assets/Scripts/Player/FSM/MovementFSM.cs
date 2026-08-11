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
        // fsm.AddTransition

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
    
}
