using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonoManager : Singleton<MonoManager>
{
    private UnityAction updateAction;
    private UnityAction fixedUpdateAction;
    private UnityAction lateUpdateAction;

    #region  生命周期
    void Update()
    {
        updateAction?.Invoke();
    }
    void FixedUpdate()
    {
        fixedUpdateAction?.Invoke();
    }
    void LateUpdate()
    {
        lateUpdateAction?.Invoke();
    }
    #endregion
    #region 注册事件
    public void AddListenerUpdate(UnityAction action)
    {
        updateAction += action;
    }
    public void AddListenerFixedUpdate(UnityAction action)
    {
        fixedUpdateAction += action;
    }
    public void AddListenerLateUpdate(UnityAction action)
    {
        lateUpdateAction += action;
    }
    #endregion
    #region 注销事件
    public void RemoveUpdate(UnityAction action)
    {
        updateAction -= action;
    }
    public void RemoveFixedUpdate(UnityAction action)
    {
        fixedUpdateAction -= action;
    }
    public void RemoveLateUpdate(UnityAction action)
    {
        lateUpdateAction -= action;
    }
    #endregion
    #region 清空
    public void Clear()
    {
        updateAction = null;
        fixedUpdateAction = null;
        lateUpdateAction = null;
    }
    #endregion
}