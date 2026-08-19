using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class CrouchingState : IState
{
    PlayerFSMContext ctx;
    private float maxSpeed;
    Vector2 moveInput;
    public CrouchingState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }
    public void OnEnter()
    {
        //动画机
        ctx.animator.SetBool(AnimatorID.IsCrouchingID, true);
        //设置最大速度为蹲下速度
        maxSpeed = ctx.CrouchSpeed;

    }

    public void OnExit()
    {
        ctx.animator.SetBool(AnimatorID.IsCrouchingID, false);
    }

    public void OnFixedUpdate()
    {
        Move();
    }

    public void OnUpdate()
    {
        PlayerInputData inputData = ctx.input;
        moveInput = inputData.moveInput;
        inputData.moveInput = Vector2.zero;//清空输入，防止重复读取
        //根据输入值调整当前速度,手柄推动的幅度越大，速度越快
        ctx.targetSpeed = moveInput.magnitude*maxSpeed;

    }
    #region Methods
     void Move()
    {
        ctx.curSpeed = Mathf.Lerp(ctx.curSpeed, ctx.targetSpeed, 3f * Time.fixedDeltaTime);
        if (ctx.curSpeed < 0.1f)
        {
            ctx.curSpeed = 0f;
        }

        ctx.animator.SetFloat(AnimatorID.NormalSpeedID, ctx.curSpeed);
        
        //TODO:修改八向位移速时修改,0.2f为魔法数字，绝不是懒不想定义变量
        ctx.animator.SetFloat(AnimatorID.HorizontalSpeedID,moveInput.x,0.2f,Time.fixedDeltaTime);
        ctx.animator.SetFloat(AnimatorID.ForwardSpeedID,moveInput.y,0.2f,Time.fixedDeltaTime);
        
        //获取相机的前向和右向向量
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;camRight.y = 0;
        camForward.Normalize();camRight.Normalize();
        
        //TODO:后续有合适动画改为根运动方式，现在就是屎
        if(ctx.canRoate){
            //移动反向
            Vector3 moveDir = (camForward*moveInput.y + camRight*moveInput.x).normalized;
            //旋转
            if (moveDir != Vector3.zero && ctx.curSpeed > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                ctx.playerTransform.rotation = Quaternion.Slerp(ctx.playerTransform.rotation, targetRotation, 6f * Time.fixedDeltaTime);
            }
        }
    }

    #endregion
}
