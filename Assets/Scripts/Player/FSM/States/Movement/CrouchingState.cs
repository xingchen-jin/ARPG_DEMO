using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class CrouchingState : StateBase<PlayerFSMContext>
{
    private float maxSpeed;
    Vector2 moveInput;
    private float idleToMoveThreshold = 0.4f;//移动速度阈值，低于该值认为是静止状态，高于该值认为是移动状态
    private Vector3 originalControllerCenter;
    private float originalControllerHeight;
    private float originalControllerRadius;

    private Vector3 idleControllerCenter;
    private float idleControllerHeight;
    private float idleControllerRadius;
    private Vector3 movingControllerCenter;
    private float movingControllerHeight;
    private float movingControllerRadius;

    public override void OnEnter()
    {
        //动画机
        ctx.animator.SetBool(AnimatorID.IsCrouchingID, true);
        //设置最大速度为蹲下速度
        maxSpeed = ctx.CrouchSpeed;
        SetCrouchingControllerSize();



    }

    public override void OnExit()
    {
        ctx.animator.SetBool(AnimatorID.IsCrouchingID, false);
        ctx.controller.height = originalControllerHeight;
        ctx.controller.center = originalControllerCenter;
        ctx.controller.radius = originalControllerRadius;
    }

    public override void OnFixedUpdate()
    {
        Move();
    }

    public override void OnUpdate()
    {
        // //根据头顶位置获取胶囊体高度（性能开销过大）
        // float headHeight = ctx.headPivot.position.y - ctx.playerTransform.position.y;
        // ctx.controller.height = headHeight;
        // ctx.controller.center = new Vector3(ctx.controller.center.x, headHeight / 2f, ctx.controller.center.z);

        PlayerInputData inputData = ctx.input;
        moveInput = inputData.moveInput;
        inputData.moveInput = Vector2.zero;//清空输入，防止重复读取
        //根据输入值调整当前速度,手柄推动的幅度越大，速度越快
        ctx.targetSpeed = moveInput.magnitude*maxSpeed;

    }

    public override void OnLateUpdate()
    {
        if(ctx.curSpeed > idleToMoveThreshold)
        {
            ctx.controller.height = movingControllerHeight;
            ctx.controller.center = movingControllerCenter;
            ctx.controller.radius = movingControllerRadius;
        }else
        {
            ctx.controller.height = idleControllerHeight;
            ctx.controller.center = idleControllerCenter;
            ctx.controller.radius = idleControllerRadius;
        }
    }

    #region Methods
    void SetCrouchingControllerSize()
    {
        //保存当前胶囊体高度和半径
        originalControllerHeight = ctx.controller.height;
        originalControllerCenter = ctx.controller.center;
        originalControllerRadius = ctx.controller.radius;
        //根据不同的移动状态设置胶囊体高度和半径
        idleControllerCenter = new Vector3(ctx.controller.center.x, ctx.controller.center.y * ctx.crouchingHeightMult_Idle, ctx.controller.center.z);
        idleControllerHeight = ctx.controller.height * ctx.crouchingHeightMult_Idle;
        idleControllerRadius = ctx.controller.radius * ctx.crouchingRadiusMult_Idle;
        movingControllerCenter = new Vector3(ctx.controller.center.x, ctx.controller.center.y * ctx.crouchingHeightMult_Fwd, ctx.controller.center.z);
        movingControllerHeight = ctx.controller.height * ctx.crouchingHeightMult_Fwd;
        movingControllerRadius = ctx.controller.radius * ctx.crouchingRadiusMult_Fwd;

        //修改胶囊体高度
        ctx.controller.height = idleControllerHeight;
        ctx.controller.center = idleControllerCenter;
        //修改胶囊体半径
        ctx.controller.radius = idleControllerRadius;
    }
     void Move()
    {
        ctx.curSpeed = Mathf.Lerp(ctx.curSpeed, ctx.targetSpeed, 3f * Time.fixedDeltaTime);
        if (ctx.curSpeed < 0.01f)
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
