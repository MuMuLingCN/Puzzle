using UnityEngine;

/// <summary>
/// 资源管理器辅助类，提供静态方法便捷访问Game实例的资源加载功能
/// </summary>
public static class ResourceManager
{
    /// <summary>
    /// 加载预制体
    /// </summary>
    public static GameObject LoadPrefab(string prefabName)
    {
        return Game.Instance?.LoadPrefab(prefabName);
    }

    /// <summary>
    /// 实例化预制体
    /// </summary>
    public static GameObject InstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation)
    {
        return Game.Instance?.InstantiatePrefab(prefabName, position, rotation);
    }

    /// <summary>
    /// 实例化预制体（默认位置和旋转）
    /// </summary>
    public static GameObject InstantiatePrefab(string prefabName)
    {
        return InstantiatePrefab(prefabName, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 实例化预制体（指定父对象）
    /// </summary>
    public static GameObject InstantiatePrefab(string prefabName, Transform parent)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab != null)
        {
            return Object.Instantiate(prefab, parent);
        }
        return null;
    }

    /// <summary>
    /// 加载精灵
    /// </summary>
    public static Sprite LoadSprite(string spriteName)
    {
        return Game.Instance?.LoadSprite(spriteName);
    }

    /// <summary>
    /// 加载音频
    /// </summary>
    public static AudioClip LoadAudio(string audioName)
    {
        return Game.Instance?.LoadAudio(audioName);
    }

    /// <summary>
    /// 通用资源加载
    /// </summary>
    public static T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        return Game.Instance?.LoadResource<T>(path);
    }

    /// <summary>
    /// 加载所有指定类型的资源
    /// </summary>
    public static T[] LoadAllResources<T>(string path) where T : UnityEngine.Object
    {
        return Resources.LoadAll<T>(path);
    }

    /// <summary>
    /// 从精灵图集中加载精灵
    /// </summary>
    public static Sprite LoadSpriteFromAtlas(string atlasPath, string spriteName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(atlasPath);
        foreach (var sprite in sprites)
        {
            if (sprite.name == spriteName)
            {
                return sprite;
            }
        }
        return null;
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    public static void LoadSceneAsync(string sceneName, System.Action onComplete = null)
    {
        if (Game.Instance != null)
        {
            Game.Instance.StartCoroutine(LoadSceneWithCallback(sceneName, onComplete));
        }
    }

    /// <summary>
    /// 异步加载场景并回调
    /// </summary>
    private static System.Collections.IEnumerator LoadSceneWithCallback(string sceneName, System.Action onComplete)
    {
        yield return Game.Instance.LoadSceneAsync(sceneName);
        onComplete?.Invoke();
    }

    /// <summary>
    /// 清除资源缓存
    /// </summary>
    public static void ClearCache()
    {
        Game.Instance?.ClearCache();
    }

    /// <summary>
    /// 卸载未使用的资源
    /// </summary>
    public static void UnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// 强制垃圾回收
    /// </summary>
    public static void ForceGC()
    {
        System.GC.Collect();
    }

    /// <summary>
    /// 释放内存资源
    /// </summary>
    public static void ReleaseMemory()
    {
        UnloadUnusedAssets();
        ForceGC();
        Debug.Log("内存资源已释放");
    }
}
