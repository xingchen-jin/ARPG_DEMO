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
    #region 地面检测
    private float groundCheckOffset;
    

    #endregion

    void Awake()
    {
        input = new PlayerInputData();
        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        ctx.controller = controller;

        //给FSMContext赋值
        ctx.animator = animator;
        ctx.playerTransform = transform;
        ctx.input = input;
        ctx.controller = controller;

        //地面检测
        groundCheckOffset = controller.radius + 0.1f;//设置偏移量为胶囊体半径+0.1f，避免检测到自身

    }
    void Start()
    {
        ctx.verticalSpeed = 0;
        ctx.useRootMotion = true;
        ctx.weapon.SetActive(false);

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
        // controller.SimpleMove(animator.velocity);
        if(!ctx.useRootMotion)
        {
            controller.Move(new Vector3(ctx.planarDisplacement.x, ctx.verticalSpeed * Time.fixedDeltaTime, ctx.planarDisplacement.z));
        }
        else
        {
            controller.Move(animator.deltaPosition + new Vector3(0, ctx.verticalSpeed * Time.fixedDeltaTime, 0));
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
    private void OnDestroy()
    {
        playerInputMap.Disable();
    }


     void OnAnimatorIK(int layerIndex)
    {
       
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

    }

    bool CheckGrounded()
    {
        if(Physics.SphereCast(transform.position + Vector3.up * groundCheckOffset, controller.radius, Vector3.down, out RaycastHit hit, groundCheckOffset-controller.radius + 2*controller.skinWidth))
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
        //TODO: 地面检测后续更改，重力效果也可以改
        if (!controller.isGrounded)
        {
            float gravityMultiplier = ctx.verticalSpeed > 0 ? ctx.jumpGravityMultiplier : ctx.fallGravityMultiplier;
            ctx.verticalSpeed += ctx.baseGravity * gravityMultiplier * Time.fixedDeltaTime;

        }
    }
}
