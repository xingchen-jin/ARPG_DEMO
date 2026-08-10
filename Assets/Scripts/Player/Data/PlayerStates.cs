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
    private FSM fsm;
    private PlayerFSMContext playerFSMContext;
    private Vector2 moveInput;

    
    public LocomotionState(FSM fsm)
    {
        this.fsm = fsm;
        this.playerFSMContext = fsm.context as PlayerFSMContext;
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

    public void OnInput(PlayerInputData inputData)
    {
        MoveInput(inputData);
        RifleInput(inputData);
    }

    public void OnUpdate()
    {
        
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

        playerFSMContext.animator.SetFloat("Speed", playerFSMContext.curSpeed);
        playerFSMContext.rb.velocity = new Vector3(moveInput.x * playerFSMContext.curSpeed, playerFSMContext.rb.velocity.y, moveInput.y * playerFSMContext.curSpeed);
        //获取相机的前向和右向向量
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;camRight.y = 0;
        camForward.Normalize();camRight.Normalize();

        //移动反向
        Vector3 moveDir = (camForward*moveInput.y + camRight*moveInput.x).normalized;
        //旋转
        if (moveDir != Vector3.zero && playerFSMContext.curSpeed > 0.1f)
        {
            Debug.Log("移动方向: " + moveDir);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            // playerFSMContext.rb.MoveRotation(Quaternion.Slerp(playerFSMContext.rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            playerFSMContext.playerTransform.rotation = Quaternion.Slerp(playerFSMContext.playerTransform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }
    void MoveInput(PlayerInputData inputData)
    {
        moveInput = inputData.moveInput;
        inputData.moveInput = Vector2.zero;//清空输入，防止重复读取

        //根据输入值调整当前速度,手柄推动的幅度越大，速度越快
        playerFSMContext.targetSpeed = moveInput.magnitude*playerFSMContext.RunSpeed;
    }
    void RifleInput(PlayerInputData inputData)
    {
        Debug.Log("RifleInput: " + inputData.RifleInput);

        playerFSMContext.animator.SetBool("Rifle",inputData.RifleInput);
        inputData.RifleInput = !inputData.RifleInput;
    }
    #endregion
}