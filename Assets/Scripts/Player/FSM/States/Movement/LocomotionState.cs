using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEditor.Callbacks;
using UnityEngine;


/// <summary>
/// 基础移动状态
/// </summary>
public class LocomotionState : StateBase<PlayerFSMContext>
{
    private Vector2 moveInput;
    private float maxSpeed;

    #region IState
    public override void OnEnter()
    {
        // moveInput = Vector2.zero;//初始化移动输入为零，防止在进入移动状态时立即移动,这也导致进入移动状态至少隔离一帧才会移动
        maxSpeed = ctx.runSpeedActual;
    }

    public override void OnExit()
    {
        
    }

    public override void OnFixedUpdate()
    {
        Move();
    }



    public override void OnUpdate()
    {
        //获取当前奔跑速度
        maxSpeed = ctx.runSpeedActual;
        //检查是否可以攀爬
        checkClimb();
        //输入处理
        PlayerInputData inputData = ctx.input;
        moveInput = inputData.moveInput;
        inputData.moveInput = Vector2.zero;//清空输入，防止重复读取
        //根据输入值调整当前速度,手柄推动的幅度越大，速度越快
        ctx.targetSpeed = moveInput.magnitude*maxSpeed;

    }
    #endregion

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
    void checkClimb()
    {
        // 检查是否可以攀爬
        if (ctx.climbDetector.TryGetClimbInfo(out ClimbType climbType, out Vector3 wallPoint, out Vector3 wallNormal))
        {
            ctx.climbType = climbType;
            ctx.canClimb = true;
            ctx.wallPoint = wallPoint;
            ctx.wallNormal = wallNormal;
            Debug.Log("可以攀爬");
        }
        else
        {
            ctx.climbType = ClimbType.None;
            ctx.canClimb = false;
            ctx.wallPoint = Vector3.zero;
            ctx.wallNormal = Vector3.zero;
            Debug.Log("不可以攀爬");
        }
    }

    #endregion
}