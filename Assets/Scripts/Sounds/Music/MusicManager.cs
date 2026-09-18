using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : BaseManager<MusicManager>
{
   MusicManager() { }
   private AudioSource audioSource;
   private readonly String path = "Music/";
   private float volume = 0.1f;

   #region 公有方法
   /// <summary>
   /// 播放音乐
   /// </summary>
   /// <param name="name">音乐文件名</param>
   public void PlayMusic(String name)
    {
        if(audioSource == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Music";
            GameObject.DontDestroyOnLoad(obj);
            audioSource = obj.AddComponent<AudioSource>();
        }

        //TODO:EditorRea(仅开发)
        audioSource.clip = EditorResManager.Instance.LoadEditorRes<AudioClip>( string.Format("{0}{1}", path, name));
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
    }
    /// <summary>
    /// 停止音乐
    /// </summary>
    public void StopMusic()
    {
        if(audioSource == null)
        {
            return;
        }
        audioSource.Stop();
    }
    /// <summary>
    /// 暂停音乐
    /// </summary>
    public void PauseMusic()
    {
        if(audioSource == null)
        {
            return;
        }
        audioSource.Pause();
    }
    /// <summary>
    /// 改变音乐音量
    /// </summary>
    /// <param name="v">音量值</param>
    public void ChangeMusicValue(float v)
    {
        volume = v;
        if(audioSource == null)
        {
            return;
        }
        audioSource.volume = volume;
    }
   #endregion

   #region 私有方法
   #endregion
}
