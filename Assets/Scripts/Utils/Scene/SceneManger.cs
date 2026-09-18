using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace XC_Framework
{
    public class SceneManger : BaseManager<SceneManger>
    {
        private SceneManger() { }
        private readonly float _progressThreshold = 0.1f; // 进度阈值，避免频繁触发事件
        #region 公共方法
        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>
        public void LoadSceneAsync(string sceneName, UnityAction callBack = null)
        {
            MonoManager.Instance.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, callBack));
        }
        private IEnumerator LoadSceneAsyncCoroutine(string sceneName, UnityAction callBack)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            SceneLoadChangeEvent sceneLoadChangeEvent = new SceneLoadChangeEvent(0);
            //每帧检测加载进度并触发事件
            while (!async.isDone)
            {
                //呼叫事件中心发送加载进度
                if (Math.Abs(sceneLoadChangeEvent.progress - async.progress/0.9f) > _progressThreshold)
                {
                    sceneLoadChangeEvent.progress = async.progress/0.9f;
                    EventCenter.EventTrigger<SceneLoadChangeEvent>(sceneLoadChangeEvent);
                }
                yield return null;
            }
            //结束加载传送最后一帧的进度
            sceneLoadChangeEvent.progress = 1;
            EventCenter.EventTrigger<SceneLoadChangeEvent>(sceneLoadChangeEvent);
            callBack?.Invoke();
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callBack"></param>   
        public void LoadScene(string sceneName, UnityAction callBack = null)
        {
            SceneManager.LoadScene(sceneName);
            callBack?.Invoke();
        }
        #endregion
    }
}
