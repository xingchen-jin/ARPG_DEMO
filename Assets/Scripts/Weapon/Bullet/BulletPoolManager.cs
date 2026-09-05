using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : Singleton<BulletPoolManager>
{
    [System.Serializable]
    public class BulletPoolConfig
    {
        public WeaponType weaponType;

        [Header("子弹炮弹")]
        public GameObject bulletPrefab;
        public int maxSize = 100;    
        public int defaultCapacity = 30;
    }
    
    public List<BulletPoolConfig> bulletPoolConfigs;
    // public List<BulletPoolConfig> bulletFireEffectPoolConfigs;
    // public List<BulletPoolConfig> bulletHitEffectPoolConfigs;

    private ObjectPoolManager<WeaponType, Bullet> bulletPoolManager = new ObjectPoolManager<WeaponType, Bullet>();
    // private ObjectPoolManager<WeaponType,> bulletFireEffectPool = new ObjectPoolManager<WeaponType, GameObject>();
    //  private ObjectPoolManager<WeaponType, GameObject> bulletHitEffectPool = new ObjectPoolManager<WeaponType, GameObject>();
    protected override void Awake()
    {
        base.Awake();
        foreach (var config in bulletPoolConfigs)
        {
            //设置父物体
            Transform parent = new GameObject($"BulletPool_{config.weaponType}").transform;
            parent.SetParent(transform);
            //建池
            bulletPoolManager.RegisterPool(config.weaponType, 
            createFunc: () =>
            {
                GameObject bulletObj = Instantiate(config.bulletPrefab, parent);
                Bullet bullet = bulletObj.GetComponent<Bullet>();
                bullet.weaponType = config.weaponType;
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
    /// <summary>
    /// 新增一个子弹对象池
    /// </summary>
    /// <param name="weaponType"> 武器类型 </param>
    /// <param name="bulletPrefab"> 子弹预制体 </param>
    /// <param name="defaultCapacity"> 默认容量 </param>
    /// <param name="maxSize"> 最大容量 </param>
    public void AddBulletPool(BulletPoolConfig config)
    {
        //设置父物体
        Transform parent = new GameObject($"BulletPool_{config.weaponType}").transform;
        parent.SetParent(transform);
        //建池
        bulletPoolManager.RegisterPool(config.weaponType, 
        createFunc: () =>
        {
            GameObject bulletObj = Instantiate(config.bulletPrefab, parent);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.weaponType = config.weaponType;
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

    /// <summary>
    /// 获取一个子弹
    /// </summary>
    /// <param name="weaponType"> 武器类型 </param>
    /// <returns> 子弹对象 </returns>
    public Bullet GetBullet(WeaponType weaponType)
    {
        return bulletPoolManager.Get(weaponType);
    }
    public void ReleaseBullet(WeaponType weaponType, Bullet bullet)
    {
        bulletPoolManager.Release(weaponType, bullet);
    }
    
}
