using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class ClimbState : StateBase
{
    PlayerFSMContext ctx;
    Vector3 leftHandTargetPos;//左手目标位置
    bool lastOpenGravity;//记录上一次的重力开关状态
    bool lastBuiltinRootMotion;//记录上一次的动画自带根运动开关状态
    public ClimbState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
        
    }
    public override void OnEnter()
    {
        
        lastOpenGravity = ctx.openGravity;
        ctx.openGravity = false; //关闭重力，避免在攀爬时
        lastBuiltinRootMotion = ctx.builtinRootMotion;
        ctx.builtinRootMotion = true; //启用动画自带的根运动

        ctx.animator.SetInteger(AnimatorID.ClimbTypeID, (int)ctx.climbType);
        Debug.Log("进入攀爬状态，攀爬类型：" + ctx.climbType);

        ctx.input.jumpInput = false; //重置跳跃输入，避免重复触发
        ctx.playerTransform.rotation = Quaternion.Lerp(ctx.playerTransform.rotation, Quaternion.LookRotation(-ctx.wallNormal), 0.5f); //旋转角色面向墙面
        ctx.controller.enabled = false; //禁用角色控制器，避免在攀爬时受到碰撞体影响
        leftHandTargetPos = ctx.wallPoint + Vector3.Cross(-ctx.wallNormal, Vector3.up) * 0.3f; //计算左手目标位置，偏移墙面法线方向
        
        
         ctx.animator.MatchTarget(leftHandTargetPos, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.1f); //匹配左手位置
        ctx.animator.MatchTarget(leftHandTargetPos-Vector3.up*0.18f, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0.1f, 0.3f); //匹配左手位置
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
        Debug.Log(ctx.animator.isMatchingTarget);//检查是否正在匹配目标
        Debug.Log(ctx.animator.IsInTransition(0));//检查是否正在过渡
        // ctx.animator.MatchTarget(leftHandTargetPos, Quaternion.identity, AvatarTarget.LeftHand, new MatchTargetWeightMask(Vector3.one, 0f), 0f, 0.1f); //匹配左手位置
        Debug.DrawLine(ctx.wallPoint, leftHandTargetPos, Color.green, 2f); //绘制左手目标位置的调试线
    }

}