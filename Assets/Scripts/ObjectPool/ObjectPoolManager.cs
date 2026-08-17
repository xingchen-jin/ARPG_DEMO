using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager<TKey,T>where T : Component
{
    private Dictionary<TKey, ObjectPool<T>> pools = new Dictionary<TKey, ObjectPool<T>>();
    //注册对象池
    public void RegisterPool(TKey key, Func<T>createFunc,Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, bool collectionCheck = false, int defaultCapacity = 10, int maxSize = 10000)
    {
        if (pools.ContainsKey(key))
        {
            Debug.LogWarning($"对象池{key}已存在，无法重复注册");
            return;
        }
        ObjectPool<T> pool = new ObjectPool<T>(createFunc, onGet, onRelease, onDestroy, collectionCheck, defaultCapacity, maxSize);
        pools.Add(key, pool);
        
    }

    //获取对象
    public T Get(TKey key)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"对象池{key}不存在，无法获取对象");
            return null;
        }
        return pools[key].Get();
    }

    //释放对象
    public void Release(TKey key, T obj)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"对象池{key}不存在，无法释放对象");
            return;
        }
        pools[key].Release(obj);
    }



}
