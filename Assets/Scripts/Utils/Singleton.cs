using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject(typeof(T).Name).AddComponent<T>();// 自动创建一个新的GameObject并添加T类型的组件
            }
            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance != null){
            Destroy(gameObject);
            return;
        }
        instance = (T)this;
        DontDestroyOnLoad(gameObject);// 保持单例对象在场景切换时不被销毁

    }
    public static bool IsInstanceExists()
    {
        return instance != null;
    }
    
    // 在销毁时清除单例实例
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

}
