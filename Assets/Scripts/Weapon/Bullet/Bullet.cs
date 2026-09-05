using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public WeaponType weaponType;
    private Transform bulletTransform;
    [SerializeField]
    private float lifeTime = 5f;
    /// <summary>
    /// 子弹的刚体组件
    /// </summary>
    private Rigidbody rb;
    [Header("击中特效")]
    [SerializeField]private GameObject hitEffectPrefab;//击中特效预制体
    [SerializeField]private float hitEffectDuration = 0.4f;//击中特效持续时间 

    [Header("开火特效")]
    [SerializeField]private GameObject fireEffectPrefab;//开火特效预制体
    [SerializeField]private float fireEffectDuration = 0.3f;//开火特效持续时间

    [Header("子弹伤害")]
    private int damage = 10;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bulletTransform = transform;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 碰撞检测
    /// </summary>
    /// <param name="collision">碰撞信息</param>
    void OnCollisionEnter(Collision collision)
    {
        // 1. 生成击中特效
        if (hitEffectPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion hitRotation = Quaternion.LookRotation(contact.normal);
            GameObject hitEffect = Instantiate(hitEffectPrefab, contact.point, hitRotation);
            Destroy(hitEffect, hitEffectDuration);
        }

        // 2. TODO: 处理伤害逻辑
        // 可以通过collision.gameObject获取被击中的对象，并调用其受伤方法
        Debug.Log($"子弹击中: {collision.gameObject.name}, 造成伤害: {damage}");
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(damage, gameObject);
        }

        // 3. 回收子弹到对象池
        BulletPoolManager.Instance.ReleaseBullet(weaponType, this);
    }
    public void Init(Vector3 position, Vector3 direction, float speed, float damage = 10)
    {
        // 1. 先更新子弹自身位置和朝向
        transform.position = position;
        transform.forward = direction;          // 立即设置朝向
        this.damage = (int)damage;              // 更新伤害值

        // 2. 设置刚体速度
        rb.velocity = direction * speed;

        // 3. 生成开火特效（此时子弹朝向已正确）
        if (fireEffectPrefab != null)
        {
            // 使用 direction 计算旋转，确保特效方向与发射方向一致
            Quaternion fireRotation = Quaternion.LookRotation(direction);
            GameObject fireEffect = Instantiate(fireEffectPrefab, position, fireRotation);
            Destroy(fireEffect, fireEffectDuration);
        }

        // 4. 启动生命周期协程
        StopAllCoroutines();                    // 确保之前可能残留的协程被清理（对象池复用时很重要）
        StartCoroutine(DestroyAfterTime());
    }
    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        //TODO: 回收对象到对象池
        BulletPoolManager.Instance.ReleaseBullet(weaponType, this);
    }
}
