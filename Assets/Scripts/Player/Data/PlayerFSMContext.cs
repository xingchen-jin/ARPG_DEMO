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
    [HideInInspector]public float curSpeed;

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
    [SerializeField] private float jumpForce;
    public float JumpForce{
        get { return jumpForce; }
    }
    #endregion

    #region 状态机参数
    [HideInInspector]public bool isGrounded;
    [HideInInspector]public bool isJumping;
    // [HideInInspector]public bool isFirearm; //持枪状态
    // [HideInInspector]public bool isAiming;  //正在瞄准，区分于输入瞄准请求
    [HideInInspector]public bool isFiring;  
    [HideInInspector]public bool canRun;
    [HideInInspector]public bool canRoate;//TODO：判断是否可以旋转，后续如果考虑新加状态则废弃

    #endregion


    #region 类型引用
    [HideInInspector]public PlayerInputData input;
    #endregion

    #region 组件引用
    [HideInInspector]public CharacterController controller;
    [HideInInspector]public Animator animator;
    [HideInInspector]public Transform playerTransform;

    public TwoBoneIKConstraint leftHandIK;
    public TwoBoneIKConstraint rightHandIK;

    #endregion
}
