using System.Collections;
using System.Collections.Generic;
using MyFSM;
using UnityEngine;

public class WeaponAimingState : StateBase
{
    private PlayerFSMContext ctx;
    
    private float roundsPerMinute = 600f; 
    private float FireInterval => 60f / roundsPerMinute; 
    private float gunTimer = 0f; 

    public WeaponAimingState(PlayerFSMContext ctx)
    {
        this.ctx = ctx;
    }

    public override void OnEnter()
    {
        //旋转到相机前方
        Camera cam = CameraManager.Instance.MainCamera;
        if(cam!=null)
        {
            //计算俯角
            Vector3 targetPosition = GetAimTargetPosition();
            AlignPlayerToTarget(targetPosition);
            
        }
        else
        {
            Debug.LogWarning("没有找到主相机，请确保场景中有一个标记为MainCamera的相机。");
        }
        
       ctx.animator.SetBool(AnimatorID.IsFirearmID, false);
       ctx.animator.SetBool(AnimatorID.IsAimingID, true);
       //rigging
        ctx.leftHandIK.weight = 1;
        ctx.rightHandIK.weight = 1;
        ctx.handAim.weight = 1;

       ctx.canRun = false;
       ctx.canRoate = false;

        CameraManager.Instance.SwitchToAimCamera();

        ctx.weapon.SetActive(true);
    }

    public override void OnExit()
    {
        ctx.animator.SetBool(AnimatorID.IsAimingID, false);
        ctx.canRun = true;
        ctx.canRoate = true;

        CameraManager.Instance.SwitchToNormalCamera();

        ctx.handAim.weight = 0;
    }

    public override void OnFixedUpdate()
    {
        Vector2 delta = ctx.input.lookInput*ctx.mouseSensitivity*Time.fixedDeltaTime;
        //水平旋转
        ctx.playerTransform.Rotate(Vector3.up, delta.x);

        //俯角旋转
        ctx.aimPitch -= delta.y;
        ctx.aimPitch = Mathf.Clamp(ctx.aimPitch, ctx.aimPitchMin, ctx.aimPitchMax);
        ctx.aimPivot.localRotation = Quaternion.Euler(ctx.aimPitch, 0, 0);

    }

    public override void OnLateUpdate()
    {
        if (ctx.input.fireInput)
        {
            Fire();

        }
    }

    #region Methods
    private Vector3 GetAimTargetPosition()
    {
        Camera cam = Camera.main; // 当前自由视角相机
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
            return hit.point;
        else
            return ray.GetPoint(50f); // 未命中时取远处固定点
    }
    private void AlignPlayerToTarget(Vector3 targetPosition)
    {
        Transform playerTransform = ctx.playerTransform;
        Transform aimPivot = ctx.aimPivot;
        //计算从当前位置指向目标的方向
        Vector3 direction = (targetPosition - playerTransform.position).normalized;
        //水平方向
        Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;
        if(horizontalDir.sqrMagnitude < 0.001f)
            horizontalDir = playerTransform.forward; //避免方向为零向量
        //设置角色的水平旋转，使其面向目标
        playerTransform.rotation = Quaternion.LookRotation(horizontalDir);
        // 计算俯仰角（角度制）
        ctx.aimPitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
        ctx.aimPitch = Mathf.Clamp(ctx.aimPitch, ctx.aimPitchMin, ctx.aimPitchMax);
        // 设置 AimPivot 本地旋转（注意符号，可能需要 -pitch）
        aimPivot.localRotation = Quaternion.Euler(ctx.aimPitch, 0, 0);
    }


    private void Fire()
    {
        if (gunTimer <= 0f)
        {
            //获取子弹对象并初始化
            Bullet bullet = BulletPoolManager.Instance.GetBullet(BulletType.Rifle);
            Vector3 firePosition = ctx.firePoint.position;
            Vector3 targetPosition = CameraManager.Instance.AimTargetTransform.position;
            // Vector3 targetPosition = GetAimTargetPosition();
            Vector3 fireDirection = (targetPosition - firePosition).normalized;

            bullet.Init(firePosition, fireDirection, 40f);//TODO: 硬编码，后续可以考虑在武器数据中配置子弹速度
            SoundEffectPoolManager.Instance.OnPlayerSound(SoundEffectType.RifleFire, firePosition);
            //开火计时器
            gunTimer = FireInterval;
        }else
        {
            gunTimer -= Time.deltaTime;
        }
    }

    #endregion

    
}
