using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MyFSM;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    PlayerInputData input;
    PlayerInputMap playerInputMap;
    Animator animator;
    CharacterController controller;
    private MovementFSM movementFSM;
    private WeaponFSM weaponFSM;

    public PlayerFSMContext ctx;

    void Start()
    {
        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        ctx.controller = controller;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

        //给FSMContext赋值
        ctx.animator = animator;
        ctx.playerTransform = transform;
        ctx.input = input;
        ctx.controller = controller;

        //初始化状态机
        movementFSM = new MovementFSM(ctx);
        movementFSM.Start();
    
        weaponFSM = new WeaponFSM(ctx);
        weaponFSM.Start();

        //IKGoal
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        
    }

    #region InputSystem事件回调
    public void OnMove(InputAction.CallbackContext context) {
        ctx.input.moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.jumpInput = true;
        }
    }
    public void OnFirearm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.RifleInput = animator.GetBool("isFirearm") ? false : true;
        }
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.aimInput = animator.GetBool("isAiming") ? false : true;
        }
    }
    #endregion


    void Update()
    {
        movementFSM.OnUpdate();
        weaponFSM.OnUpdate();
        // movementFSM.OnInput(playerInputData);
        // Debug.Log("当前状态: " + movementFSM.curState.GetType().Name);
    }
    void OnAnimatorMove()
    {
        controller.SimpleMove(animator.velocity);
    }
    void FixedUpdate()
    {
        movementFSM.OnFixedUpdate();
        weaponFSM.OnFixedUpdate();
    }
    private void OnDestroy()
    {
        playerInputMap.Disable();
    }


     void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("OnAnimatorIK");
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

    }
}
