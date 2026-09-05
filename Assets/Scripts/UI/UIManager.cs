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
    private Dictionary<string, UIBasePanel> panelDic = new Dictionary<string, UIBasePanel>();
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
    /// <param name="level"></param>
    /// <param name="callback"></param>
    /// <param name="isSync"></param>
    public void ShowPanel<T>( UILevel level = UILevel.Middle,UnityAction<T> callback = null,bool isSync = false)where T:UIBasePanel
    {
        //获取面板名 预设体名必须和面板类名一致 
        string panelName = typeof(T).Name;
        UIBasePanel panel;
        //加载面板预设体
        if(panelDic.TryGetValue(panelName, out panel))
        {
            //面板已经存在 直接显示
            panel.ShowMe();
            panel.gameObject.SetActive(true);
            callback?.Invoke(panel as T);
        }
        else
        {
            //面板不存在 需要加载
            //TODO: 这里可以使用异步加载的方式
            GameObject panelObj = GameObject.Instantiate(ResManager.Instance.Load<GameObject>($"{uiPath}{panelName}"), GetLayerParent(level), false);
            if (panelObj == null)
            {
                Debug.LogError($"面板{panelName}加载失败");
                return;
            }
            panel = panelObj.GetComponent<T>();
            if (panel == null)
            {
                Debug.LogError($"面板{panelName}上没有挂载{typeof(T)}组件");
                return;
            }
            //面板显示时会调用一次默认的显示逻辑
            panel.ShowMe();
            //执行回调
            callback?.Invoke(panel as T);
            //存储到字典中
            panelDic.Add(panelName, panel);
        }
            // if (panel != null)
            // {
            //     if (panel.BlocksPlayerInput)
            //     {
            //         EventCenter.EventTrigger<SwitchInputModeEvent>(new SwitchInputModeEvent(InputMode.UI));
            //     }
            // }
    
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void HidePanel<T>() where T : UIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            //执行默认隐藏逻辑
            panelDic[panelName].HideMe();
            //面板存在，隐藏它
            panelDic[panelName].gameObject.SetActive(false);
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
            GameObject.Destroy(panelDic[panelName].gameObject);
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
    /// <returns></returns>
    public T GetPanel<T>() where T : UIBasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        else
        {
            Debug.LogWarning($"面板{panelName}不存在");
            return null;
        }
    }
}
