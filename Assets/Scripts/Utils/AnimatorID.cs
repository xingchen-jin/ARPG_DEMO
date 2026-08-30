using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class AnimatorID
{
    public static readonly int VerticalSpeedID = Animator.StringToHash("Vertical Speed");
    public static readonly int HorizontalSpeedID = Animator.StringToHash("Horizontal Speed");
    public static readonly int ForwardSpeedID = Animator.StringToHash("Forward Speed");
    public static readonly int NormalSpeedID = Animator.StringToHash("Normal Speed");
    public static readonly int IsFirearmID = Animator.StringToHash("isFirearm");
    public static readonly int IsAimingID = Animator.StringToHash("isAiming");
    public static readonly int IsJumpingID = Animator.StringToHash("isJumping");
    public static readonly int IsCrouchingID = Animator.StringToHash("isCrouching");
    public static readonly int ClimbTypeID = Animator.StringToHash("ClimbType");
    public static readonly int ReloadTriggerID = Animator.StringToHash("ReloadTrigger");
} 
