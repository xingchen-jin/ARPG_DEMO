using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class ClimbState : StateBase <PlayerFSMContext>
{
    Vector3 leftHandTargetPos;//左手目标位置
    Vector3 rightHandTargetPos;//右手目标位置
    Vector3 rightFootTargetPos;//右脚目标位置
    ClimbType climbType;//攀爬类型
    bool lastOpenGravity;//记录上一次的重力开关状态
    bool lastBuiltinRootMotion;//记录上一次的动画自带根运动开关状态
    const float matchEndTime = 0.1f; //MatchTarget匹配窗口的结束时间（归一化时间）
    const float matchOffestHeight = 0.1f; //MatchTarget匹配目标位置的偏移距离

    public override void OnEnter()
    {
        climbType = ctx.climbType;
        lastOpenGravity = ctx.openGravity;
        ctx.openGravity = false; //关闭重力，避免在攀爬时
        lastBuiltinRootMotion = ctx.builtinRootMotion;
        ctx.builtinRootMotion = true; //启用动画自带的根运动

        ctx.animator.SetInteger(AnimatorID.ClimbTypeID, (int)climbType);
        Debug.Log("进入攀爬状态，攀爬类型：" + climbType);

        ctx.input.jumpInput = false; //重置跳跃输入，避免重复触发
        ctx.playerTransform.rotation = Quaternion.Lerp(ctx.playerTransform.rotation, Quaternion.LookRotation(-ctx.wallNormal), 0.5f); //旋转角色面向墙面
        ctx.controller.enabled = false; //禁用角色控制器，避免在攀爬时受到碰撞体影响
        switch(climbType)
        {
            case ClimbType.LowClimb:
                leftHandTargetPos = ctx.wallPoint + Vector3.Cross(-ctx.wallNormal, Vector3.up) * 0.3f + Vector3.up*matchOffestHeight; //计算左手目标位置，偏移墙面法线方向
                break;
            case ClimbType.HighClimb:
                rightHandTargetPos = ctx.wallPoint + Vector3.Cross(ctx.wallNormal, Vector3.up) * 0.3f; //计算右手目标位置，偏移墙面法线方向
                rightFootTargetPos = ctx.wallPoint + Vector3.down * 1.2f; //计算右脚目标位置,在墙面顶端下方1.2米处
                break;
            case ClimbType.Vault:
                rightHandTargetPos = ctx.wallPoint; //计算右手目标位置，偏移墙面法线方向
                break;
        }
        //ctx.CoroutineRunner.StartCoroutine(MatchTargetWaitCoroutine()); //启动协程，等待动画过渡完成后再注册 MatchTarget
        
    }

    public override void OnExit()
    {
        ctx.openGravity = lastOpenGravity; //恢复重力开关状态
        ctx.builtinRootMotion = lastBuiltinRootMotion; //恢复动画自带根运动开关状态
        ctx.animator.SetInteger(AnimatorID.ClimbTypeID, 0);
        
        ctx.controller.enabled = true; //启用角色控制器
    }
    public override void OnUpdate()
    {
        //核心修复：Climb 状态真正开始播放后才逐帧注册 MatchTarget。
        //IsTag("Climb") 在过渡期间为 false（此时 GetCurrentAnimatorStateInfo 返回源状态），
        //因此能保证匹配窗口与 Climb_Low 动画的归一化时间对齐。
        AnimatorStateInfo stateInfo = ctx.animator.GetCurrentAnimatorStateInfo(0);
        if(ctx.animator.IsInTransition(0))
        {
            return;
        }

        if (stateInfo.IsTag("Climb") && climbType == ClimbType.LowClimb)
        {
            if(ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= matchEndTime)
            {
                ctx.animator.MatchTarget(leftHandTargetPos, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, matchEndTime); //匹配左手位置
                ctx.animator.MatchTarget(leftHandTargetPos+Vector3.up*0.18f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), matchEndTime,matchEndTime+0.2f ); //匹配左手位置，延迟0.2秒开始匹配
            }
        }
        else if (stateInfo.IsTag("Climb") && climbType == ClimbType.HighClimb)
        { 
            if(ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.13f)
                ctx.animator.MatchTarget(rightFootTargetPos+Vector3.up*matchOffestHeight, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.13f); //匹配左手位置
            if(ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.32f)
                ctx.animator.MatchTarget(rightHandTargetPos+Vector3.up*matchOffestHeight, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.2f,0.32f ); //匹配左手位置，延迟0.2秒开始匹配
        }
        else if (stateInfo.IsTag("Climb") && climbType == ClimbType.Vault)
        {
            if(ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f && ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.2f)
                ctx.animator.MatchTarget(rightHandTargetPos, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.1f, 0.2f); //匹配左手位置
            if(ctx.animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.45f)
                ctx.animator.MatchTarget(rightHandTargetPos+Vector3.up*0.1f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.35f,0.45f ); //匹配左手位置，延迟0.2秒开始匹配
        }
        Debug.Log(ctx.animator.isMatchingTarget);//检查是否正在匹配目标
        Debug.Log(ctx.animator.IsInTransition(0));//检查是否正在过渡
        //Debug.DrawLine(ctx.wallPoint, leftHandTargetPos, Color.green, 2f); //绘制左手目标位置的调试线
    }

    // IEnumerator MatchTargetWaitCoroutine()
    // {
    //     float waitTime = 0f;
    //     while (ctx.animator.IsInTransition(0))
    //     {
    //         yield return null; //等待下一帧
    //         waitTime += Time.deltaTime;
    //     }
    //     Debug.Log($"Wait time: {waitTime}");
    //     yield return new WaitForEndOfFrame(); //等待一帧，确保动画状态机已经切换到 Climb 状态
    //     waitTime+= Time.deltaTime;

    //     switch (climbType)
    //     {
    //         case ClimbType.LowClimb:
    //             ctx.animator.MatchTarget(leftHandTargetPos, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, matchEndTime); //匹配左手位置
    //             ctx.animator.MatchTarget(leftHandTargetPos+Vector3.up*0.18f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), matchEndTime,matchEndTime+0.2f ); //匹配左手位置，延迟0.2秒开始匹配
    //             break;
    //         case ClimbType.HighClimb:
    //             ctx.animator.MatchTarget(rightFootTargetPos, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.13f); //匹配右手位置
    //             ctx.animator.MatchTarget(rightHandTargetPos, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.2f,0.32f ); //匹配右手位置，延迟0.2秒开始匹配
    //             break;
    //         case ClimbType.Vault:
    //             ctx.animator.MatchTarget(rightHandTargetPos, Quaternion.identity, AvatarTarget.RightFoot, new MatchTargetWeightMask(Vector3.one, 0f), 0.1f, 0.2f); //匹配左手位置
    //             ctx.animator.MatchTarget(rightHandTargetPos+Vector3.up*0.1f, Quaternion.identity, AvatarTarget.RightHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.35f,0.45f ); //匹配左手位置，延迟0.2秒开始匹配
    //             break;
    //     }
    //     yield break;
       
    // }

}