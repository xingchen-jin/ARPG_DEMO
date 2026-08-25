using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    public WeaponType weaponType;
    [SerializeField]
    private float lifeTime = 5f;
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Init(Vector3 position, Vector3 direction, float speed)
    {
        transform.position = position;
        rb.velocity = direction * speed;
        //转向
        transform.forward = direction;
        
        StartCoroutine(DestroyAfterTime());
    }
    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        //TODO: 回收对象到对象池
        BulletPoolManager.Instance.ReleaseBullet(weaponType, this);
    }

}
