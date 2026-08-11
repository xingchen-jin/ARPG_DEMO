using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEditor.Callbacks;
using UnityEngine;


/// <summary>
/// 基础移动状态
/// </summary>
public class LocomotionState : IState
{
    private PlayerFSMContext playerFSMContext;
    private Vector2 moveInput;
    
    
    public LocomotionState(PlayerFSMContext ctx)
    {
        this.playerFSMContext = ctx;

    }

    #region IState
    public void OnEnter()
    {
        // moveInput = Vector2.zero;//初始化移动输入为零，防止在进入移动状态时立即移动,这也导致进入移动状态至少隔离一帧才会移动
    }

    public void OnExit()
    {
        
    }

    public void OnFixedUpdate()
    {
        Move();
    }


    public void OnUpdate()
    {

        PlayerInputData inputData = playerFSMContext.input;
        moveInput = inputData.moveInput;
        inputData.moveInput = Vector2.zero;//清空输入，防止重复读取
        //根据输入值调整当前速度,手柄推动的幅度越大，速度越快
        playerFSMContext.targetSpeed = moveInput.magnitude*playerFSMContext.RunSpeed;

        Debug.Log("targetSpeed "+playerFSMContext.targetSpeed);
    }
    #endregion

    #region Methods
    void Move()
    {
        playerFSMContext.curSpeed = Mathf.Lerp(playerFSMContext.curSpeed, playerFSMContext.targetSpeed, 3f * Time.fixedDeltaTime);
        if (playerFSMContext.curSpeed < 0.1f)
        {
            playerFSMContext.curSpeed = 0f;
        }

        playerFSMContext.animator.SetFloat(AnimatorID.NormalSpeedID, playerFSMContext.curSpeed);
        
        //TODO:修改八向位移速时修改,0.2f为魔法数字，绝不是懒不想定义变量
        playerFSMContext.animator.SetFloat(AnimatorID.HorizontalSpeedID,moveInput.x,0.2f,Time.fixedDeltaTime);
        playerFSMContext.animator.SetFloat(AnimatorID.VerticalSpeedID,moveInput.y,0.2f,Time.fixedDeltaTime);

        
        //获取相机的前向和右向向量
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;camRight.y = 0;
        camForward.Normalize();camRight.Normalize();
        
        //TODO:后续有合适动画改为根运动方式
        if(playerFSMContext.canRoate){
            //移动反向
            Vector3 moveDir = (camForward*moveInput.y + camRight*moveInput.x).normalized;
            //旋转
            if (moveDir != Vector3.zero && playerFSMContext.curSpeed > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                playerFSMContext.playerTransform.rotation = Quaternion.Slerp(playerFSMContext.playerTransform.rotation, targetRotation, 6f * Time.fixedDeltaTime);
            }
        }
    }

    #endregion
}