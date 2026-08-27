using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResManager : BaseManager<ResManager>
{
    Dictionary<string, Object> resDict = new Dictionary<string, Object>();
    private ResManager()
    {
        
    }
    public T Load<T>(string path) where T : Object
    {
        if (resDict.ContainsKey(path))
        {
            return resDict[path] as T;
        }
        else
        {
            T res = Resources.Load<T>(path);
            if (res != null)
            {
                resDict.Add(path, res);
                return res;
            }
            else
            {
                Debug.LogError("资源加载失败，路径为：" + path);
                return null;
            }
        }
    }
}
