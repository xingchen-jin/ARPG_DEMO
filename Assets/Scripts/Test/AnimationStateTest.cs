using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateTest : MonoBehaviour
{
    Animator animator;
        PlayerInputMap playerInputMap;
    void Start()
    {
        animator = GetComponent<Animator>();
        playerInputMap = new PlayerInputMap();
        playerInputMap.Enable();
    }
    void Update()
    {
        if (playerInputMap.Player.Move.ReadValue<Vector2>() != Vector2.zero)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
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
