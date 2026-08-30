using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponReloadState : StateBase<PlayerFSMContext>
{
    private readonly float runSpeedMultiplier = 0.5f; //换弹时奔跑速度的缩放倍数
    private float preRunSpeed; //换弹前的奔跑速度
    public override void OnEnter()
    {
        //动画参数
        ctx.animator.SetTrigger(AnimatorID.ReloadTriggerID);
        ctx.animator.SetBool(AnimatorID.IsFirearmID, true);
        ctx.animator.SetBool(AnimatorID.IsAimingID, false);

        //IK参数
        ctx.rightHandIK.weight = 0;
        ctx.weaponController.SetLeftHandIKWeight(0);
        preRunSpeed = ctx.runSpeedActual; //记录换弹前的奔跑速度
        //换弹时降低奔跑速度
        ctx.runSpeedActual = ctx.RunSpeedBase * runSpeedMultiplier; 

        
    }
    public override void OnExit()
    {
        ctx.runSpeedActual = preRunSpeed; //恢复奔跑速度
    }
    

    #region Methods
    #endregion

}
