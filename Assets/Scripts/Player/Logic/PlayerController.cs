using System;
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
    private ClimbDetector climbDetector;//攀爬检测器
    private WeaponController weaponController;
   

    public PlayerFSMContext ctx;
    #region 地面检测
    [SerializeField]private float groundCheckOffset = 0.1f;
    

    #endregion

    void Awake()
    {
        input = new PlayerInputData();
        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        climbDetector = GetComponent<ClimbDetector>();
        weaponController = GetComponent<WeaponController>();

        ctx.weaponController = weaponController;
        ctx.controller = controller;
        //给FSMContext赋值
        ctx.animator = animator;
        ctx.playerTransform = transform;
        ctx.input = input;
        ctx.controller = controller;
        ctx.climbDetector = climbDetector;
        
        //地面检测
        // groundCheckOffset = controller.radius + 0.1f;//设置偏移量为胶囊体半径+0.1f，避免检测到自身

    }

     void Start()
    {
        //初始化武器相关,根据实际武器调整初始值
        // ctx.weaponModel = ctx.weapon.GetComponentInChildren<WeaponBehavior>().gameObject;
        // WeaponBehavior firearm = ctx.weaponModel.GetComponent<WeaponBehavior>();
        // ctx.firePoint = firearm.firePoint;
        // ctx.leftHandIK.data.target = firearm.LeftHandIKTarget;

        ctx.verticalSpeed = 0;
        ctx.useRootMotion = true;
        weaponController.EnableWeapon(false);

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
            ctx.input.crouchInput = false; //按下跳跃键时取消下蹲状态
    
        }
    }
    /// <summary>
    /// 切换武器事件回调,目前只支持单一武器切换（空手/拿枪），后续可扩展为多武器切换
    /// </summary>
    /// <param name="context"></param>
    public void OnFirearm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.RifleInput = animator.GetBool("isFirearm") ? false : true;
            EventCenter.Instance.EventTrigger<WeaponType>(EventType.SwitchWeaponRequest, WeaponType.Rifle);//TODO:测试武器切换，后续需要扩展
        }
    }
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.aimInput = animator.GetBool("isAiming") ? false : true;
        }
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        ctx.input.lookInput = context.ReadValue<Vector2>();
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                ctx.input.fireInput = true;
                break;
            case InputActionPhase.Canceled:
                ctx.input.fireInput = false;
                break;
            case InputActionPhase.Performed:
                ctx.input.fireInput = true;
                break;
        }
    }
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.reloadInput = true;
        }
    }

    //TODO:待解决在跳跃等不应该触发下蹲的状态下，按下下蹲键会触发下蹲状态的问题
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ctx.input.crouchInput = !ctx.input.crouchInput;
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
        if(ctx.builtinRootMotion)
        {
            ctx.animator.ApplyBuiltinRootMotion();
            return;
        }
        // controller.SimpleMove(animator.velocity);
        if(!ctx.useRootMotion)
        {
            controller.Move(new Vector3(ctx.planarDisplacement.x, ctx.verticalSpeed * Time.fixedDeltaTime, ctx.planarDisplacement.z));
        }
        else
        {
            controller.Move(animator.deltaPosition+ new Vector3(0, ctx.verticalSpeed * Time.fixedDeltaTime, 0));
        }
        // controller.Move(animator.deltaPosition + new Vector3(0, ctx.verticalSpeed * Time.fixedDeltaTime, 0));
     
    }
    void FixedUpdate()
    {
        ctx.isGrounded = CheckGrounded();
        Debug.Log("是否在地面上: " + ctx.isGrounded);
        Gravity();
        animator.SetFloat(AnimatorID.VerticalSpeedID, ctx.verticalSpeed);

        movementFSM.OnFixedUpdate();
        weaponFSM.OnFixedUpdate();
    }
    void LateUpdate()
    {
        movementFSM.OnLateUpdate();
        weaponFSM.OnLateUpdate();
    }
    private void OnDestroy()
    {
        playerInputMap.Disable();
    }

    //  void OnAnimatorIK(int layerIndex)
    // {
    //     //脚部IK
    //     animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
    //     animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
    //     animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
    //     animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
    //
    //     //手部IK：攀爬时关闭（ctx.handIKEnabled），避免与MatchTarget冲突
    //     float handWeight = ctx.handIKEnabled ? 1f : 0f;
    //     animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
    //     animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);
    //     animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handWeight);
    //     animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handWeight);
    // }
    private void OnSwitchWeapon()
    {
        
    }


    bool CheckGrounded()
    {
        if(Physics.SphereCast(transform.position + Vector3.up * (controller.radius + groundCheckOffset), controller.radius, Vector3.down, out RaycastHit hit, groundCheckOffset + 2*controller.skinWidth))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 重力模拟，只模拟玩家在空中的重力，不影响在地面时的值
    /// </summary>
    void Gravity()
    {
        if (!ctx.openGravity) {
            return;
        }
        //TODO: 地面检测后续更改，重力效果也可以改
        if (!ctx.isGrounded)
        {
            float gravityMultiplier = ctx.verticalSpeed > 0 ? ctx.jumpGravityMultiplier : ctx.fallGravityMultiplier;
            ctx.verticalSpeed += ctx.baseGravity * gravityMultiplier * Time.fixedDeltaTime;

        }else if(ctx.verticalSpeed < 0)
        {
            ctx.verticalSpeed = 0;
        }
    }

    void checkedclimb()
    {
        // 检查是否可以攀爬
        if (climbDetector.TryGetClimbInfo(out ClimbType climbType, out Vector3 wallPoint, out Vector3 wallNormal))
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
}
