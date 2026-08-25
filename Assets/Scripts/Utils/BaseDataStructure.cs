using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XC_Framework
{   
    /// <summary>
    /// 一个通用的Pair结构体，用于存储两个相关联的值。
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    [System.Serializable]
    public struct Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 first, T2 second)
        {
            this.first = first;
            this.second = second;
        }
    }


}
