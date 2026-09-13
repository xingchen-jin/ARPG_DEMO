using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BaseManager<T> where T : class
{
    private static T instance;

    public static T Instance
    {
        get
        {
            Type type = typeof(T);
            if (instance == null)
            {
                ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                            null,
                                                            Type.EmptyTypes,
                                                            null);
                if (info != null)
                    instance = info.Invoke(null) as T;
                else
                    Debug.LogError("没有得到对应的无参构造函数");
            }
            return instance;
        }
    }
}
