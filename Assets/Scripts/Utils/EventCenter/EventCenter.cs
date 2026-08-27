using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class EventInfoBase { }
public class EventInfo<T> : EventInfoBase
{
    //真正观察者 对应的 函数信息 记录在其中
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}
public class EventInfo<T1,T2> : EventInfoBase
{
    public UnityAction<T1,T2> actions;

    public EventInfo(UnityAction<T1,T2> action)
    {
        actions += action;
    }
}
public class EventInfo : EventInfoBase
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

public class EventCenter : BaseManager<EventCenter>
{
    //用于记录对应事件 关联的 对应的逻辑
    private Dictionary<EventType, EventInfoBase> eventDic = new Dictionary<EventType, EventInfoBase>();

    private EventCenter() { }

    /// <summary>
    /// 触发事件 
    /// </summary>
    /// <param name="eventName">事件名字</param>
    public void EventTrigger<T>(EventType eventName, T info)
    {
        //存在关心我的人 才通知别人去处理逻辑
        if (eventDic.ContainsKey(eventName))
        {
            //去执行对应的逻辑
            (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
        }
    }
    public void EventTrigger<T1,T2>(EventType eventName, T1 info1,T2 info2)
    {
        //存在关心我的人 才通知别人去处理逻辑
        if (eventDic.ContainsKey(eventName))
        {
            //去执行对应的逻辑
            (eventDic[eventName] as EventInfo<T1,T2>).actions?.Invoke(info1,info2);
        }
    }
    public void EventTrigger(EventType eventName)
    {
        //存在关心我的人 才通知别人去处理逻辑
        if (eventDic.ContainsKey(eventName))
        {
            //去执行对应的逻辑
            (eventDic[eventName] as EventInfo).actions?.Invoke();
        }
    }
    /// <summary>
    /// 注册事件监听者
    /// </summary>
    /// <typeparam name="T">传递的参数类型</typeparam>
    /// <param name="eventName">事件类型</param>
    /// <param name="action">监听的事件</param>
    public void AddListener<T>(EventType eventName, UnityAction<T> action)
    {
        //如果没有这个事件的监听者
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic.Add(eventName, new EventInfo<T>(action));
        }
        else
        {
            (eventDic[eventName] as EventInfo<T>).actions += action;
        }
    }
    public void AddListener<T1,T2>(EventType eventName, UnityAction<T1,T2> action)
    {
        //如果没有这个事件的监听者
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic.Add(eventName, new EventInfo<T1,T2>(action));
        }
        else
        {
            (eventDic[eventName] as EventInfo<T1,T2>).actions += action;
        }
    }
    public void AddListener(EventType eventName, UnityAction action)
    {
        //如果没有这个事件的监听者
        if (!eventDic.ContainsKey(eventName))
        {
            eventDic.Add(eventName, new EventInfo(action));
        }
        else
        {
            (eventDic[eventName] as EventInfo).actions += action;
        }
    }
    /// <summary>
    /// 移除事件监听者
    /// </summary>
    /// <typeparam name="T">传递的参数类型</typeparam>
    /// <param name="eventName">事件类型</param>
    /// <param name="action">要移除的监听事件</param>
    public void RemoveListener<T>(EventType eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).actions -= action;
        }
    }
    public void RemoveListener<T1,T2>(EventType eventName, UnityAction<T1,T2> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T1,T2>).actions -= action;
        }
    }
    public void RemoveListener(EventType eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).actions -= action;
        }
    }

    /// <summary>
    /// 清空单一事件的所有监听
    /// </summary>
    /// <param name="eventName"></param>
    public void ClearListener(EventType eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
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
