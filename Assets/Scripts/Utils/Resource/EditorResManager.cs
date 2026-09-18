using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器资源管理器
/// 注意：只有在开发时能使用该管理器加载资源 用于开发功能
/// </summary>
public class EditorResManager : BaseManager<EditorResManager>
{
    private readonly string rootPath = "Assets/Editor/ArtRes/";

    private EditorResManager() { }
    #region 公共方法

    /// <summary>
    /// 加载单个资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadEditorRes<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        string suffixName = GetSuffixName(typeof(T));
        return AssetDatabase.LoadAssetAtPath<T>(rootPath + path + suffixName);
#else
    return null;
#endif
    }

    /// <summary>
    ///加载图集中的单个子图片
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="spriteName">精灵名称</param>
    /// <returns></returns>
    public Sprite LoadSprite(string path, string spriteName)
    {
#if UNITY_EDITOR
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            if (spriteName == item.name)
                return item as Sprite;
        }
        return null;
#else
    return null;
#endif
    }

    /// <summary>
    /// 加载图集文件中的所有子图片并返回给外部
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public Dictionary<string, Sprite> LoadSprites(string path)
    {
#if UNITY_EDITOR
        Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            spriteDic.Add(item.name, item as Sprite);
        }
        return spriteDic;
#else
    return null;
#endif
    }
    #endregion
    #region 私有方法
    private string GetSuffixName(System.Type type)
    {
        switch (type.Name)
        {
            case nameof(GameObject):
                return ".prefab";
            case nameof(Material):
                return ".mat";
            case nameof(Texture):
                return ".png";
            case nameof(AudioClip):
                return ".mp3";
            default:
                return string.Empty;
        }
    }

    #endregion
}
