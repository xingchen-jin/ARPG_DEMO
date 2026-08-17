using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : Singleton<BulletPoolManager>
{
    [System.Serializable]
    public class BulletPoolConfig
    {
        public BulletType bulletType;
        public GameObject bulletPrefab;
        public int maxSize = 100;    
        public int defaultCapacity = 30;
    }
    
    public List<BulletPoolConfig> bulletPoolConfigs;
    private ObjectPoolManager<BulletType, Bullet> bulletPoolManager = new ObjectPoolManager<BulletType, Bullet>();
    protected override void Awake()
    {
        base.Awake();
        foreach (var config in bulletPoolConfigs)
        {
            //设置父物体
            Transform parent = new GameObject($"BulletPool_{config.bulletType}").transform;
            parent.SetParent(transform);
            //建池
            bulletPoolManager.RegisterPool(config.bulletType, 
            createFunc: () =>
            {
                GameObject bulletObj = Instantiate(config.bulletPrefab, parent);
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                return bullet;
            }, onGet: (bullet) =>
            {
                bullet.gameObject.SetActive(true);
            }, onRelease: (bullet) =>
            {
                bullet.gameObject.SetActive(false);
            }, onDestroy: (bullet) =>
            {
                Destroy(bullet.gameObject);
            }, collectionCheck: false,
            defaultCapacity: config.defaultCapacity, maxSize: config.maxSize);
        }
    }

    public Bullet GetBullet(BulletType bulletType)
    {
        return bulletPoolManager.Get(bulletType);
    }
    public void ReleaseBullet(BulletType bulletType, Bullet bullet)
    {
        bulletPoolManager.Release(bulletType, bullet);
    }
    
}
