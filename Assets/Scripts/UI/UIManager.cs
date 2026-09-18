using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum UILevel
{
    Bottom = 0,
    Middle = 1,
    Top = 2,
    /// <summary>
    /// 系统层 最高层
    /// </summary>
    System = 3,
}
public class UIManager : BaseManager<UIManager>
{
    private abstract class UiBaseInfo{}
    /// <summary>
    /// UI信息类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private class UIInfo<T> : UiBaseInfo where T : UIBasePanel
    {
        public UnityAction<T> callback;
        public T panel;
        public bool isHide;
        public UIInfo(UnityAction<T> callback)
        {
            this.callback += callback;
            isHide = false;
        }
    }
    //ui必要组件
    //private Camera uiCamera;
    private Canvas uiCanvas;
    private EventSystem uiEventSystem;
    //层级父对象
    private RectTransform bottomLayer;
    private RectTransform middleLayer;
    private RectTransform topLayer;
    private RectTransform systemLayer;

    /// <summary>
    /// 存储所有的面板对象
    /// </summary>
    private Dictionary<string, UiBaseInfo> panelDic = new Dictionary<string, UiBaseInfo>();
    private static string uiPath = "UI/";
    private static string canvasPath = "UI/Canvas";
    private static string eventSystemPath = "UI/EventSystem";

    private UIManager()
    {
        Init();
    }

    private void Init()
    {
        //动态创建唯一的Canvas和EventSystem（摄像机）
        //uiCamera = GameObject.Instantiate(ResManager.Instance.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();
        //ui摄像机过场景不移除 专门用来渲染UI面板
        //GameObject.DontDestroyOnLoad(uiCamera.gameObject);

        //动态创建Canvas
        uiCanvas = GameObject.Instantiate(ResManager.Instance.Load<GameObject>(canvasPath)).GetComponent<Canvas>();
        //设置使用的UI摄像机
        //uiCanvas.worldCamera = uiCamera;
        //canvas过场景不移除
        GameObject.DontDestroyOnLoad(uiCanvas.gameObject);

        //动态创建EventSystem
        uiEventSystem = GameObject.Instantiate(ResManager.Instance.Load<GameObject>(eventSystemPath)).GetComponent<EventSystem>();
        //eventSystem过场景不移除
        GameObject.DontDestroyOnLoad(uiEventSystem.gameObject);

        //创建层级父对象
        bottomLayer = CreateUILayer("BottomLayer", uiCanvas.transform);
        middleLayer = CreateUILayer("MiddleLayer", uiCanvas.transform);
        topLayer = CreateUILayer("TopLayer", uiCanvas.transform);
        systemLayer = CreateUILayer("SystemLayer", uiCanvas.transform);

    }
    private RectTransform CreateUILayer(string layerName, Transform parent)
    {
        GameObject layerObj = new GameObject(layerName);
        layerObj.transform.SetParent(parent);
        RectTransform rectTransform = layerObj.AddComponent<RectTransform>();
        //重置锚点，位置和尺寸（覆盖全屏）
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one; //重置缩放


        return rectTransform;
    }
    /// <summary>
    /// 获取层级父对象
    /// </summary>
    /// <param name="level">级别</param>
    /// <returns></returns>
    public  RectTransform GetLayerParent(UILevel level)
    {
        switch (level)
        {
            case UILevel.Bottom:
                return bottomLayer;
            case UILevel.Middle:
                return middleLayer;
            case UILevel.Top:
                return topLayer;
            case UILevel.System:
                return systemLayer;
            default:
                return null;
        }
    }
    /// <summary>
    /// 显示面板
    /// </summary>
    /// <param name="panelName">面板名称</param>
    /// <param name="level">ui层级（默认Middle）</param>
    /// <param name="callback">回调</param>
    /// <param name="isSync">是否同步加载</param>
    public void ShowPanel<T>( UILevel level = UILevel.Middle,UnityAction<T> callback = null,bool isSync = false)where T:UIBasePanel
    {
        //获取面板名 预设体名必须和面板类名一致 
        string panelName = typeof(T).Name;
        //加载面板预设体
        if(panelDic.TryGetValue(panelName, out UiBaseInfo uiInfo))
        {
            //获取ui信息类
            UIInfo<T> info = panelDic[panelName] as UIInfo<T>;
            if(info.panel == null)
            {
                //如果隐藏后又显示把isHide设为false
                info.isHide = false;
                //还在加载中
                if(callback != null)
                {
                    info.callback += callback;
                }
                return;
            }
            if(info.panel.gameObject.activeSelf == false)
            {
                info.panel.gameObject.SetActive(true);
            }
            info.panel.ShowMe();
            callback?.Invoke(info.panel as T);
        }
        else
        {
            //面板不存在 需要加载
            panelDic.Add(panelName,new UIInfo<T>(callback));
            //异步加载
            if (!isSync)
            {
                ResManager.Instance.LoadAsync<GameObject>($"{uiPath}{panelName}", (value)=>{
                    GameObject panelObj = GameObject.Instantiate(value);
                    panelObj.transform.SetParent(GetLayerParent(level) == null ? middleLayer : GetLayerParent(level), false);
                    T panel = panelObj.GetComponent<T>();
                    if (panel == null)
                    {
                        Debug.LogError($"面板{panelName}上没有挂载{typeof(T)}组件");
                        return;
                    }
                    //获取ui信息类
                    UIInfo<T> info = panelDic[panelName] as UIInfo<T>;
                    //执行回调
                    info.callback?.Invoke(panel as T);
                    //清空回调，避免内存泄漏
                    info.callback = null; 
                    //面板加载完成后根据isHide的值决定是否显示
                    if (info.isHide)
                    {
                        if(panelObj.activeSelf)
                            panelObj.SetActive(false);
                        panel.HideMe();
                        info.isHide = false; //重置isHide状态
                    }else
                    {
                        if(!panelObj.activeSelf)
                            panelObj.SetActive(true);
                        panel.ShowMe();
                    }

                    //存储到字典中
                    info.panel = panel;
                    // panelDic[panelName]
                    });
            }
            //同步加载
            else
            {
                GameObject panelObj = GameObject.Instantiate(ResManager.Instance.Load<GameObject>($"{uiPath}{panelName}"));
                panelObj.transform.SetParent(GetLayerParent(level) == null ? middleLayer : GetLayerParent(level), false);
                T panel = panelObj.GetComponent<T>();
                if (panel == null)
                {
                    Debug.LogError($"面板{panelName}上没有挂载{typeof(T)}组件");
                    return;
                }
                //获取ui信息类
                UIInfo<T> info = panelDic[panelName] as UIInfo<T>;
                //执行回调
                info.callback?.Invoke(panel as T);
                //清空回调，避免内存泄漏
                info.callback = null; 
                //面板加载完成后根据isHide的值决定是否显示
                if (info.isHide)
                {
                    if(panelObj.activeSelf)
                        panelObj.SetActive(false);
                    panel.HideMe();
                    info.isHide = false; //重置isHide状态
                }else
                {
                    if(!panelObj.activeSelf)
                        panelObj.SetActive(true);
                    panel.ShowMe();
                }

                //存储到字典中
                info.panel = panel;
            }

        }
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="isDestory">是否销毁面板</param>
    /// <typeparam name="T"></typeparam>
    public void HidePanel<T>(bool isDestory = false) where T : UIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            UIInfo<T> info = panelDic[panelName] as UIInfo<T>;
            //面板加载完成
            if(info.panel != null)
            {
                if (isDestory)
                {
                    GameObject.Destroy(info.panel.gameObject);
                    panelDic.Remove(panelName);
                    return;
                }
                info.panel.HideMe();
                if(info.panel.gameObject.activeSelf)
                    info.panel.gameObject.SetActive(false);
            }
            //面板未加载完成
            else
            {
                //标记为隐藏
                info.isHide = true;
                info.callback = null; //清空回调，避免加载完成后执行
            }
        }
        else
        {
            Debug.LogWarning($"面板{panelName}不存在，无法隐藏");
        }
    }

    /// <summary>
    /// 销毁面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void DestoryPanel<T>() where T : UIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            //面板存在，销毁它
            if(panelDic[panelName] is UIInfo<T> info && info.panel != null)
            {
                GameObject.Destroy(info.panel.gameObject);
            }
            panelDic.Remove(panelName);
        }
        else
        {
            Debug.LogWarning($"面板{panelName}不存在，无法销毁");
        }
    }

    /// <summary>
    /// 获取面板对象
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="callback">回调函数</param>
    /// <returns></returns>
    public void GetPanel<T>(UnityAction<T> callback) where T : UIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            UIInfo<T> info = panelDic[panelName] as UIInfo<T>;
            //加载结束
            if(info.panel != null)
            {
                callback?.Invoke(info.panel);
            }
            //正在加载中
            else
            {
                //面板还在加载中，添加回调
                info.callback += callback;
            }
        }
        else
        {
            Debug.LogWarning($"面板{panelName}不存在");
        }
    }
   
    /// <summary>
    /// 为控件添加自定义事件
    /// </summary>
    /// <param name="control">待添加事件的控件</param>
    /// <param name="eventType">事件类型</param>
    /// <param name="callback">响应函数</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType eventType, UnityAction<BaseEventData> callback)
    {
        //获取或添加EventTrigger组件
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = control.gameObject.AddComponent<EventTrigger>();
        }
        //创建一个新的事件条目
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }
}
