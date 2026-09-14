using System;
using UnityEngine;
using UnityEngine.InputSystem;



[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour,IDamageable
{
    PlayerInputData input;
    Animator animator;
    CharacterController controller;
    private MovementFSM movementFSM;
    private WeaponFSM weaponFSM;
    private ClimbDetector climbDetector;//攀爬检测器
    private WeaponController weaponController;
    private InputHandler inputHandler;

    public PlayerFSMContext ctx;
    [Header("基础数值属性")]
    [Expandable][SerializeField]
    private PlayerBaseInfo_SO playerBaseInfo;
    [Header("地面检测")]
    [SerializeField]private float groundCheckOffset = 0.1f;
    

    #region Unity生命周期函数
    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        climbDetector = GetComponent<ClimbDetector>();
        weaponController = GetComponent<WeaponController>();
        inputHandler = GetComponent<InputHandler>();
        
        ctx.runSpeedActual = ctx.RunSpeedBase; //初始化当前奔跑速度为基础奔跑速度
        ctx.weaponController = weaponController;
        ctx.controller = controller;
        //给FSMContext赋值
        ctx.animator = animator;
        ctx.playerTransform = transform;
        ctx.controller = controller;
        ctx.climbDetector = climbDetector;
        ctx.input = new PlayerInputData();
        
        ctx.canJump = true;
        inputHandler.ctx = ctx; //将ctx传递给InputHandler
        
        //初始化玩家基础属性
        playerBaseInfo.currentHealth = playerBaseInfo.maxHealth;
        //TODO: 在游戏开始时，刷新UI血条，后续可以改为角色初始化时触发事件，UI监听事件刷新血条
        EventCenter.EventTrigger<UpdateHealthEvent>(new UpdateHealthEvent(playerBaseInfo.currentHealth, playerBaseInfo.maxHealth));

        //地面检测
        // groundCheckOffset = controller.radius + 0.1f;//设置偏移量为胶囊体半径+0.1f，避免检测到自身
    }
    void OnEnable()
    {
        EventCenter.AddListener<SwitchWeaponEvent>(OnSwitchWeapon);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener<SwitchWeaponEvent>(OnSwitchWeapon);
    }

    private void OnSwitchWeapon(SwitchWeaponEvent @event)
    {
        Debug.Log($"切换武器事件触发，武器类型：{@event.weaponType}");
        
        switch (@event.weaponType)
        {
            case WeaponType.Melee:
                ctx.input.RifleInput = false;
                break;
            default:
                ctx.input.RifleInput = true;
                break;
        }
//        Debug.Log($"切换武器事件处理完成，RifleInput: {ctx.input.RifleInput}");
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
        ctx.movementFSM = movementFSM;
    
        weaponFSM = new WeaponFSM(ctx);
        weaponFSM.Start();
        ctx.weaponFSM = weaponFSM;

        //IKGoal
        // animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        // animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        // animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        // animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
    }
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
//        Debug.Log("是否在地面上: " + ctx.isGrounded);
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
        
    }
    #endregion

    

    #region 动画事件回调
    public void OnAnimEvent(string eventName)
    {
        weaponFSM.OnAnimEvent(eventName);
        movementFSM.OnAnimEvent(eventName);
    }

    #endregion
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

    public void TakeDamage(float damage, GameObject attacker)
    {
        float currentHealth = playerBaseInfo.currentHealth;
        currentHealth -= damage;
        playerBaseInfo.currentHealth = currentHealth;
        if (currentHealth <= 0)
        {
            // 处理玩家死亡逻辑
            Debug.Log("玩家死亡！");
            //TODO: 可以在这里触发玩家死亡动画、游戏结束等逻辑
        }
        else
        {
            // 处理玩家受伤逻辑
            Debug.Log($"玩家受到 {damage} 点伤害！当前生命值: {currentHealth}");
        }

        //刷新UI血条
        EventCenter.EventTrigger<UpdateHealthEvent>(new UpdateHealthEvent(playerBaseInfo.currentHealth, playerBaseInfo.maxHealth));
        //TODO: 播放受伤动画



    }
}
