using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 游戏单例类，管理游戏全局状态和资源加载
/// </summary>
public class Game : MonoBehaviour
{
    private static Game _instance;
    public static Game Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Game>();
                if (_instance == null)
                {
                    GameObject gameObj = new GameObject("Game");
                    _instance = gameObj.AddComponent<Game>();
                }
            }
            return _instance;
        }
    }

    [Header("游戏设置")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("预加载设置")]
    [SerializeField] private bool preloadOnAwake = true;
    [SerializeField] private List<string> preloadScenes = new List<string>();

    // 游戏状态
    public bool IsLoading { get; private set; }
    public float LoadProgress { get; private set; }
    public string CurrentLoadInfo { get; private set; }

    // 资源缓存
    private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
    private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();

    // 事件
    public event Action OnPreloadComplete;
    public event Action<float> OnLoadProgress;
    public event Action<string> OnLoadInfoChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        Debug.Log("Game实例已创建");

        // 如果需要预加载
        if (preloadOnAwake)
        {
            StartCoroutine(PreloadResourcesAsync());
        }
    }

    /// <summary>
    /// 异步预加载游戏资源
    /// </summary>
    public IEnumerator PreloadResourcesAsync()
    {
        IsLoading = true;
        UpdateLoadInfo("开始预加载资源...");
        UpdateLoadProgress(0f);

        int totalSteps = 4;
        int currentStep = 0;

        // 步骤1: 预加载场景
        currentStep++;
        UpdateLoadInfo($"预加载场景... ({currentStep}/{totalSteps})");
        UpdateLoadProgress((float)currentStep / totalSteps);

        foreach (string sceneName in preloadScenes)
        {
            yield return LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        yield return new WaitForSeconds(0.1f);

        // 步骤2: 预加载预制体
        currentStep++;
        UpdateLoadInfo($"预加载预制体... ({currentStep}/{totalSteps})");
        UpdateLoadProgress((float)currentStep / totalSteps);

        yield return PreloadPrefabs();

        yield return new WaitForSeconds(0.1f);

        // 步骤3: 预加载精灵图片
        currentStep++;
        UpdateLoadInfo($"预加载图片资源... ({currentStep}/{totalSteps})");
        UpdateLoadProgress((float)currentStep / totalSteps);

        yield return PreloadSprites();

        yield return new WaitForSeconds(0.1f);

        // 步骤4: 预加载音频
        currentStep++;
        UpdateLoadInfo($"预加载音频资源... ({currentStep}/{totalSteps})");
        UpdateLoadProgress((float)currentStep / totalSteps);

        yield return PreloadAudio();

        yield return new WaitForSeconds(0.1f);

        // 完成加载
        IsLoading = false;
        UpdateLoadInfo("预加载完成！");
        UpdateLoadProgress(1f);

        Debug.Log("游戏资源预加载完成");
        OnPreloadComplete?.Invoke();
    }

    /// <summary>
    /// 预加载预制体
    /// </summary>
    private IEnumerator PreloadPrefabs()
    {
        // 这里可以根据实际项目需求加载特定的预制体
        // 例如：从Resources文件夹加载
        string[] prefabPaths = { "Prefabs/UI", "Prefabs/Game" };

        foreach (string path in prefabPaths)
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(path);
            foreach (var prefab in prefabs)
            {
                if (prefab != null)
                {
                    _prefabCache[prefab.name] = prefab;
                    yield return null;
                }
            }
        }
    }

    /// <summary>
    /// 预加载精灵图片
    /// </summary>
    private IEnumerator PreloadSprites()
    {
        // 从Resources文件夹加载所有精灵
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images");

        foreach (var sprite in sprites)
        {
            if (sprite != null)
            {
                _spriteCache[sprite.name] = sprite;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 预加载音频
    /// </summary>
    private IEnumerator PreloadAudio()
    {
        // 从Resources文件夹加载所有音频
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Audio");

        foreach (var audio in audioClips)
        {
            if (audio != null)
            {
                _audioCache[audio.name] = audio;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    public IEnumerator LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        UpdateLoadInfo($"正在加载场景: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, mode);

        while (!asyncLoad.isDone)
        {
            float progress = asyncLoad.progress;
            UpdateLoadProgress(progress * 0.9f); // 场景加载占90%
            yield return null;
        }

        // 场景加载完成，激活场景
        if (mode == LoadSceneMode.Additive)
        {
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
            }
        }
    }

    /// <summary>
    /// 异步加载场景到缓存
    /// </summary>
    public IEnumerator PreloadSceneAsync(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            Debug.Log($"场景 {sceneName} 已经加载");
            yield break;
        }

        UpdateLoadInfo($"预加载场景: {sceneName}");

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            UpdateLoadProgress(asyncLoad.progress * 0.9f);
            yield return null;
        }

        // 场景预加载完成，但不激活
        Debug.Log($"场景 {sceneName} 预加载完成");
    }

    /// <summary>
    /// 加载预制体
    /// </summary>
    public GameObject LoadPrefab(string prefabName)
    {
        if (_prefabCache.TryGetValue(prefabName, out GameObject prefab))
        {
            return prefab;
        }

        // 如果缓存中没有，尝试从Resources加载
        GameObject loadedPrefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
        if (loadedPrefab != null)
        {
            _prefabCache[prefabName] = loadedPrefab;
            return loadedPrefab;
        }

        Debug.LogWarning($"未找到预制体: {prefabName}");
        return null;
    }

    /// <summary>
    /// 实例化预制体
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = LoadPrefab(prefabName);
        if (prefab != null)
        {
            return Instantiate(prefab, position, rotation);
        }
        return null;
    }

    /// <summary>
    /// 加载精灵
    /// </summary>
    public Sprite LoadSprite(string spriteName)
    {
        if (_spriteCache.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }

        // 如果缓存中没有，尝试从Resources加载
        Sprite loadedSprite = Resources.Load<Sprite>($"Images/{spriteName}");
        if (loadedSprite != null)
        {
            _spriteCache[spriteName] = loadedSprite;
            return loadedSprite;
        }

        Debug.LogWarning($"未找到精灵: {spriteName}");
        return null;
    }

    /// <summary>
    /// 加载音频
    /// </summary>
    public AudioClip LoadAudio(string audioName)
    {
        if (_audioCache.TryGetValue(audioName, out AudioClip audio))
        {
            return audio;
        }

        // 如果缓存中没有，尝试从Resources加载
        AudioClip loadedAudio = Resources.Load<AudioClip>($"Audio/{audioName}");
        if (loadedAudio != null)
        {
            _audioCache[audioName] = loadedAudio;
            return loadedAudio;
        }

        Debug.LogWarning($"未找到音频: {audioName}");
        return null;
    }

    /// <summary>
    /// 通用资源加载
    /// </summary>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public IEnumerator LoadResourceAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);

        while (!request.isDone)
        {
            UpdateLoadProgress(request.progress);
            yield return null;
        }

        T resource = request.asset as T;
        onComplete?.Invoke(resource);
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearCache()
    {
        _prefabCache.Clear();
        _spriteCache.Clear();
        _audioCache.Clear();

        // 卸载未使用的资源
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("资源缓存已清除");
    }

    /// <summary>
    /// 更新加载进度
    /// </summary>
    private void UpdateLoadProgress(float progress)
    {
        LoadProgress = Mathf.Clamp01(progress);
        OnLoadProgress?.Invoke(LoadProgress);
    }

    /// <summary>
    /// 更新加载信息
    /// </summary>
    private void UpdateLoadInfo(string info)
    {
        CurrentLoadInfo = info;
        OnLoadInfoChanged?.Invoke(info);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
