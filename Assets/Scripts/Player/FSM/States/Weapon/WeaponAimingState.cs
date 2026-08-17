using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponAimingState : IState
{
    private PlayerFSMContext ctx;
    //TODO:射击相关，后续多武器时，修改为数据驱动
    private float roundsPerMinute = 600f; //每分钟射击次数
    private float FireInterval => 60f / roundsPerMinute; //射击间隔时间
    private float gunTimer = 0f; //射击计时器

    public WeaponAimingState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }

    public void OnEnter()
    {
       ctx.animator.SetBool("isFirearm", false);
       ctx.animator.SetBool("isAiming", true);
       //设置Rigging权重
        ctx.leftHandIK.weight = 1;
        ctx.rightHandIK.weight = 1;
        ctx.handAim.weight = 1;

       ctx.canRun = false;
       ctx.canRoate = false;

        CameraManager.Instance.SwitchToAimCamera();
    }

    public void OnExit()
    {
        ctx.animator.SetBool("isAiming", false);
        ctx.canRun = true;
        ctx.canRoate = true;

        CameraManager.Instance.SwitchToNormalCamera();

        ctx.handAim.weight = 0;
    }

    public void OnFixedUpdate()
    {
        Vector2 delta = ctx.input.lookInput*ctx.mouseSensitivity*Time.fixedDeltaTime;
        //水平旋转角色
        // Debug.Log($"水平旋转角色:{delta.x}");
        ctx.playerTransform.Rotate(Vector3.up, delta.x);
        //俯角旋转
        ctx.aimPitch -= delta.y;
        ctx.aimPitch = Mathf.Clamp(ctx.aimPitch, ctx.aimPitchMin, ctx.aimPitchMax);
        ctx.aimPivot.localRotation = Quaternion.Euler(ctx.aimPitch, 0, 0);

    }

    public void OnUpdate()
    {
        if (ctx.input.fireInput)
        {
            Fire();

        }

    }
    private void Fire()
    {
        if (gunTimer <= 0f)
        {
            //生成子弹
            Bullet bullet = BulletPoolManager.Instance.GetBullet(BulletType.Rifle);
            Vector3 firePosition = ctx.firePoint.position;
            Vector3 targetPosition = CameraManager.Instance.AimTargetTransform.position;
            Vector3 fireDirection = (targetPosition - firePosition).normalized;

            bullet.Init(firePosition, fireDirection, 20f);//TODO: 这里的速度可以从武器数据中获取
            SoundEffectPoolManager.Instance.OnPlayerSound(SoundEffectType.RifleFire, firePosition);
            //重置射击计时器
            gunTimer = FireInterval;
        }else
        {
            gunTimer -= Time.deltaTime;
        }
    }
}
