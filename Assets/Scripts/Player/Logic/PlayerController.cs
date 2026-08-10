using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using MyFSM;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerFSMContext : FSMContext
{
    // [HideInInspector]public PlayerInputMap inputActions;
    //玩家基础参数
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [HideInInspector]public float curSpeed;
    public float targetSpeed;

    public float WalkSpeed{
        get { return walkSpeed; }
    }
    public float RunSpeed{
        get { return runSpeed; }
    }
    [SerializeField] private float jumpForce;
    public float JumpForce{
        get { return jumpForce; }
    }

     [HideInInspector]public Rigidbody rb;
     [HideInInspector]public Animator animator;
     [HideInInspector]public Transform playerTransform;
    // public CinemachineVirtualCamera cinemachineCamera;
}

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    PlayerInputData playerInputData;
    PlayerInputMap playerInputMap;
    Animator animator;
    Rigidbody rb;
    private FSM fsm;
    public PlayerFSMContext playerFSMContext;

    void Start()
    {
        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

        //给FSMContext赋值
        playerFSMContext.rb = rb;
        playerFSMContext.animator = animator;
        playerFSMContext.playerTransform = transform;

        //初始化状态机
        fsm = new FSM(playerFSMContext);
        //添加状态
        fsm.AddState(StateType.Locomotion, new LocomotionState(fsm));

        //切换到初始状态
        fsm.SwitchState(StateType.Locomotion);

    }
    //InputSystem事件回调
    public void OnMove(InputAction.CallbackContext context) {
        playerInputData.moveInput = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerInputData.jumpInput = true;
        }
    }
    public void OnRifle(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerInputData.RifleInput = true;
        }
    }


    void Update()
    {
        fsm.OnUpdate();
        fsm.OnInput(playerInputData);
        Debug.Log("当前状态: " + fsm.curState.GetType().Name);
    }
    void FixedUpdate()
    {
        fsm.OnFixedUpdate();
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
