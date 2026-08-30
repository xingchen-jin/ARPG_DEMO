using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponBaseState : StateBase<PlayerFSMContext>
{

    public override void OnEnter()
    {
        ctx.animator.SetBool(AnimatorID.IsFirearmID, false);
        ctx.animator.SetBool(AnimatorID.IsAimingID, false);
        
        ctx.weaponController.SetLeftHandIKWeight(0);
        ctx.rightHandIK.weight = 0;
        //关闭武器显示
        ctx.canRoate = true;
        ctx.weaponController.EnableWeapon(false);
        //进入基础状态，切换为空手
        EventCenter.EventTrigger<SwitchWeaponEvent>(new SwitchWeaponEvent(WeaponType.Melee));
    }

    public override void OnUpdate()
    {
    }

}
