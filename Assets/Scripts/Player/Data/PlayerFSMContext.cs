using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[System.Serializable]
public class PlayerFSMContext : FSMContext
{
    // [HideInInspector]public PlayerInputMap inputActions;
    #region 玩家基础参数
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;

    [HideInInspector]public float curSpeed; //速度的模值
    [HideInInspector]public float targetSpeed;

    //定义瞄准时，八向移动速度
    // [HideInInspector]public float curHorizontal;
    // [HideInInspector]public float curVertical;
    // [HideInInspector]public float targetHorizontal;
    // [HideInInspector]public float targetVertical;




    public float WalkSpeed{
        get { return walkSpeed; }
    }
    public float RunSpeed{
        get { return runSpeed; }
    }
    public float CrouchSpeed{
        get { return crouchSpeed; }
    }

    #endregion

    #region 状态机参数
    // [HideInInspector]public bool isGrounded;
    // [HideInInspector]public bool isJumping;
    // [HideInInspector]public bool isFirearm; //持枪状态
    // [HideInInspector]public bool isAiming;  //正在瞄准，区分于输入瞄准请求 
    [HideInInspector]public bool canRun;
    [HideInInspector]public bool canRoate;//TODO：判断是否可以旋转，后续如果考虑新加状态则废弃
    [HideInInspector]public bool useRootMotion;

    #endregion


    #region 类型引用
    [Header("类型引用")]
    [HideInInspector]public PlayerInputData input;
    #endregion

    #region 组件引用
    [Header("组件引用")]
    [HideInInspector]public CharacterController controller;
    [HideInInspector]public Animator animator;
    [HideInInspector]public Transform playerTransform;

    public TwoBoneIKConstraint leftHandIK;
    public TwoBoneIKConstraint rightHandIK;
    public MultiAimConstraint handAim;
    
    public GameObject weapon;
    public Transform headPivot;//头顶位置


    #endregion

    #region 瞄准相关
    [Header("瞄准相关")]
    public Transform aimPivot;
    public Transform firePoint;
    // 瞄准时的俯仰角度
    public float aimPitchMin = -30f;
    public float aimPitchMax = 30f;
    public float aimPitch = 0;
    
    public float mouseSensitivity = 2f;// 鼠标灵敏度
    #endregion

    #region 重力与跳跃系统
    [Header("重力与跳跃系统")]
    public float baseGravity = -9.81f; //初始速度
    public float jumpGravityMultiplier = 1.0f;//上升时重力倍数
    public float fallGravityMultiplier = 1.8f;//下降重力倍数

    public float jumpDispMultiplier = 1.2f; //跳跃位移倍数，影响跳跃的位移量

    public float verticalSpeed = 0;
    public bool isGrounded;
    [SerializeField] private float initVerticalSpeed; //初始速度
    public float InitVerticalSpeed{
        get { return initVerticalSpeed; }
    }
    #endregion

    #region 其他参数
    [Header("其他参数")]
    [HideInInspector]public float groundCheckDistance = 0.1f; //地面检测距离

    [HideInInspector]public Vector3 planarDisplacement; //水平平面位移量,禁用根运动时使用
    public float crouchingHeightMult_Fwd = 0.52f; //下蹲时胶囊体高度缩放倍数
    public float crouchingRadiusMult_Fwd = 1.74f; //下蹲时胶囊体半径缩放倍数

    public float crouchingHeightMult_Idle = 0.75f; //下蹲时静止状态下的胶囊体高度缩放倍数
    public float crouchingRadiusMult_Idle = 1.74f; //下蹲时静止状态下的胶囊体半径缩放倍数
    #endregion
    
}
