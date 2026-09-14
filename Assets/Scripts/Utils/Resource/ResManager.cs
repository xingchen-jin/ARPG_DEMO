using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ResInfoBase
{
    public int refCount; //引用计数，记录有多少个地方在使用该资源
    public void AddRefCount()
    {
        refCount++;
    }
    public void SubRefCount()
    {
        refCount--;
        if (refCount < 0)
        {
            Debug.LogError("资源引用计数小于0，可能存在资源释放错误");
        }
    }
}
public class ResInfo<T> : ResInfoBase
{
    //资源
    public T asset;
    //主要用于异步加载结束后 传递资源到外部的委托
    public UnityAction<T> callBack;
    //用于存储异步加载时 开启的协同程序
    public Coroutine coroutine;
    public bool isDel;
}
public class ResManager : BaseManager<ResManager>
{
    //private Dictionary<string, UnityEngine.Object> resDict = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, ResInfoBase> resDict = new Dictionary<string, ResInfoBase>();
    private ResManager()
    {
        
    }
    #region 加载资源
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <param name="callBack">回调函数</param>
    /// <returns></returns>
    public void LoadAsync<T>(string path,UnityAction<T> callBack) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        if (!resDict.ContainsKey(resName))
        {
            ResInfo<T> newInfo = new ResInfo<T>();
            newInfo.callBack = callBack;
            resDict.Add(resName, newInfo);
            newInfo.AddRefCount();

            newInfo.coroutine = MonoManager.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }
        else
        {
            ResInfo<T> info = resDict[resName] as ResInfo<T>;
            info.AddRefCount();

            if (info.asset != null)
            {
                callBack?.Invoke(info.asset);
            }
            else
            {
                info.callBack += callBack;
            }
        }
    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;

        string resName = path + "_" + typeof(T).Name;
        if(resDict.ContainsKey(resName))
        {
            ResInfo<T> info = resDict[resName] as ResInfo<T>;
            info.asset = request.asset as T;

            if (info.refCount <= 0)
            {
                UnloadAsset<T>(path);
            }
            else
            {
                info.callBack?.Invoke(info.asset);
                info.callBack = null;
                info.coroutine = null;
            }

        }
    }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        String resName = path + "_" + typeof(T).Name;
        if (resDict.ContainsKey(resName))
        {
            ResInfo<T> info = resDict[resName] as ResInfo<T>;
            info.AddRefCount();
            if (info.asset != null)
            {
                return info.asset;
            }
            else
            {
                //还在加载中则强制关闭异步加载，转为同步加载
               MonoManager.Instance.StopCoroutine(info.coroutine);
                T res = Resources.Load<T>(path);
                info.asset = res;
                //还应该把那些等待着异步加载结束的委托去执行了
                info.callBack?.Invoke(res);
                //回调结束 异步加载也停了 所以清除无用的引用
                info.callBack = null;
                info.coroutine = null;
                // 并使用
                return res;
            }
        }else
        {

            //不在字典中使用同步加载
            T res = Resources.Load<T>(path);
            //记录到字典中
            ResInfo<T> info = new ResInfo<T>();
            info.asset = res;
            resDict.Add(resName, info);
            return res;
        }
    }
    #endregion
    #region 资源卸载
    /// <summary>
    /// 同步卸载指定资源
    /// </summary>
    /// <param name="path">路径</param>
    public void UnloadAsset<T>(string path,UnityAction<T> callBack = null,bool isDel = false,bool isSub = true) where T : UnityEngine.Object
    {
        String resName = path + "_" + typeof(T).Name;
        if (resDict.ContainsKey(resName))
        {
            ResInfo<T> info = resDict[resName] as ResInfo<T>;
            //记录引用为0时是否卸载资源
            info.isDel = isDel;
            if (isSub)
            {
                info.SubRefCount();
            }
            if (info.asset != null && info.refCount <= 0 && info.isDel)
            {
                Resources.UnloadAsset(info.asset as UnityEngine.Object);
                resDict.Remove(resName);
            }else if(info.asset == null)
            {
                if(callBack != null)
                {
                    info.callBack -= callBack;
                }
            }
        }
        else
        {
            Debug.LogWarning($"资源 {path} 不存在于字典中，无法卸载");
        }
    }
    /// <summary>
    /// 异步卸载未使用的Resources资源
    /// </summary>
    /// <param name="callBack">回调函数，通知卸载完成</param>
   public void UnLoadUnUsedAssets(UnityAction callBack = null)
    {
        MonoManager.Instance.StartCoroutine(ReallyUnLoadUnUsedAssets(callBack));
    }
    private IEnumerator ReallyUnLoadUnUsedAssets(UnityAction callBack = null)
    {
        //记录引用为0的资源，在字典中移除
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in resDict)
        {
            if (kvp.Value.refCount <= 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        //移除引用为0的资源
        foreach(var key in keysToRemove)
        {
            resDict.Remove(key);
        }

        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        //通知外部
        callBack?.Invoke();
    }

    /// <summary>
    /// 清空字典，卸载所有未使用的资源
    /// </summary>
    /// <param name="callBack">回调函数，通知清理完成</param>
    public void ClearDictionary(UnityAction callBack = null)
    {
       MonoManager.Instance.StartCoroutine(ReallyClearDictionary(callBack));
    }
    private IEnumerator ReallyClearDictionary(UnityAction callBack = null)
    {
        resDict.Clear();
        AsyncOperation ao = Resources.UnloadUnusedAssets();
        yield return ao;
        callBack?.Invoke();
    }

    #endregion
    #region 资源引用计数管理
    /// <summary>
    /// 获取指定资源的引用计数,-1表示资源不存在于字典中
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    public int GetRefCount<T>(string path) where T : UnityEngine.Object
    {
        string resName = path + "_" + typeof(T).Name;
        if (resDict.ContainsKey(resName))
        {
            ResInfo<T> info = resDict[resName] as ResInfo<T>;
            return info.refCount;
        }
        else
        {
            Debug.LogWarning($"资源 {path} 不存在于字典中，无法获取引用计数");
            return -1;
        }
    }
    #endregion
}
