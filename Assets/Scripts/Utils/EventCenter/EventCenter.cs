using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter : BaseManager<EventCenter>
{
    //用于记录对应事件 关联的 对应的逻辑
    private Dictionary<Type,List<Delegate>> eventDic = new Dictionary<Type,List<Delegate>>();

    private EventCenter() { }

    /// <summary>
    /// 触发事件 
    /// </summary>
    /// <param name="eventName">事件数据</param>
    public void EventTrigger<T>(T eventArgs) where T : IEvent
    {
        //存在关心我的人 才通知别人去处理逻辑
        if (eventDic.ContainsKey(typeof(T)))
        {
            List<Delegate> delegateList = eventDic[typeof(T)];
            foreach (var handler in delegateList)
            {
                ((Action<T>)handler)?.Invoke(eventArgs);
            }
        }
    }
    /// <summary>
    /// 注册事件监听者
    /// </summary>
    /// <typeparam name="T">传递的参数类型</typeparam>
    /// <param name="eventName">事件类型</param>
    /// <param name="action">监听的事件</param>
    /// <returns></returns>
    public void AddListener<T>(Action<T> handler) where T : IEvent
    {
        var type = typeof(T);
        if (!eventDic.TryGetValue(type, out List<Delegate> list))
        {
            list = new List<Delegate>();
            eventDic[type] = list;
        }
        list.Add(handler);
    }

    /// <summary>
    /// 移除事件监听者
    /// </summary>
    /// <typeparam name="T">事件类型</typeparam>
    /// <param name="handler">要移除的监听事件</param>
    public void RemoveListener<T>(Action<T> handler) where T : IEvent
    {
        if (eventDic.ContainsKey(typeof(T)))
        {
            eventDic[typeof(T)].Remove(handler);
        }
    }

    /// <summary>
    /// 清空单一事件的所有监听
    /// </summary>
    /// <typeparam name="T">事件数据包类型</typeparam>
    public void ClearListener<T>()
    {
        if (eventDic.ContainsKey(typeof(T)))
        {
            eventDic.Remove(typeof(T));
        }
    }

    /// <summary>
    /// 清空所有事件的监听
    /// </summary>
    public void ClearAllEvent()
    {
        eventDic.Clear();
    }

    
}
