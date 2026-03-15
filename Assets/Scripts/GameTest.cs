using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Game系统测试脚本
/// 用于演示和测试Game单例、资源加载和预加载功能
/// </summary>
public class GameTest : MonoBehaviour
{
    [Header("测试UI")]
    [SerializeField] private Button testLoadPrefabButton;
    [SerializeField] private Button testLoadSpriteButton;
    [SerializeField] private Button testLoadAudioButton;
    [SerializeField] private Button testSceneLoadButton;
    [SerializeField] private Button testClearCacheButton;
    [SerializeField] private TMP_Text logText;

    [Header("测试参数")]
    [SerializeField] private string testPrefabName = "Player";
    [SerializeField] private string testSpriteName = "Background";
    [SerializeField] private string testAudioName = "Click";
    [SerializeField] private string testSceneName = "Game";

    private int logCount = 0;
    private const int maxLogLines = 20;

    private void Start()
    {
        // 订阅Game事件
        if (Game.Instance != null)
        {
            Game.Instance.OnLoadProgress += OnLoadProgress;
            Game.Instance.OnLoadInfoChanged += OnLoadInfoChanged;
            Game.Instance.OnPreloadComplete += OnPreloadComplete;

            Log("Game系统测试脚本已启动");
            Log($"Game实例已存在: {Game.Instance != null}");
            Log($"正在加载: {Game.Instance.IsLoading}");
        }
        else
        {
            Log("错误: Game实例未找到！");
        }

        // 绑定按钮事件
        if (testLoadPrefabButton != null)
            testLoadPrefabButton.onClick.AddListener(TestLoadPrefab);

        if (testLoadSpriteButton != null)
            testLoadSpriteButton.onClick.AddListener(TestLoadSprite);

        if (testLoadAudioButton != null)
            testLoadAudioButton.onClick.AddListener(TestLoadAudio);

        if (testSceneLoadButton != null)
            testSceneLoadButton.onClick.AddListener(TestSceneLoad);

        if (testClearCacheButton != null)
            testClearCacheButton.onClick.AddListener(TestClearCache);
    }

    /// <summary>
    /// 测试加载预制体
    /// </summary>
    private void TestLoadPrefab()
    {
        Log($"开始测试加载预制体: {testPrefabName}");

        GameObject prefab = ResourceManager.LoadPrefab(testPrefabName);
        if (prefab != null)
        {
            Log($"✓ 预制体加载成功: {prefab.name}");

            // 实例化预制体
            GameObject instance = ResourceManager.InstantiatePrefab(testPrefabName, Vector3.zero, Quaternion.identity);
            if (instance != null)
            {
                Log($"✓ 预制体实例化成功: {instance.name}");

                // 5秒后销毁
                Destroy(instance, 5f);
                Log("测试对象将在5秒后销毁");
            }
        }
        else
        {
            Log($"✗ 预制体加载失败: {testPrefabName}");
            Log("提示: 请确保Resources/Prefabs文件夹中存在该预制体");
        }
    }

    /// <summary>
    /// 测试加载精灵
    /// </summary>
    private void TestLoadSprite()
    {
        Log($"开始测试加载精灵: {testSpriteName}");

        Sprite sprite = ResourceManager.LoadSprite(testSpriteName);
        if (sprite != null)
        {
            Log($"✓ 精灵加载成功: {sprite.name}");
            Log($"精灵尺寸: {sprite.rect.width} x {sprite.rect.height}");
        }
        else
        {
            Log($"✗ 精灵加载失败: {testSpriteName}");
            Log("提示: 请确保Resources/Images文件夹中存在该精灵");
        }
    }

    /// <summary>
    /// 测试加载音频
    /// </summary>
    private void TestLoadAudio()
    {
        Log($"开始测试加载音频: {testAudioName}");

        AudioClip audio = ResourceManager.LoadAudio(testAudioName);
        if (audio != null)
        {
            Log($"✓ 音频加载成功: {audio.name}");
            Log($"音频长度: {audio.length:F2}秒");

            // 播放音频
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audio;
            audioSource.Play();
            Log("正在播放测试音频...");

            // 播放完成后清理
            StartCoroutine(CleanupAudioSource(audioSource, audio.length + 1f));
        }
        else
        {
            Log($"✗ 音频加载失败: {testAudioName}");
            Log("提示: 请确保Resources/Audio文件夹中存在该音频");
        }
    }

    /// <summary>
    /// 清理音频源
    /// </summary>
    private System.Collections.IEnumerator CleanupAudioSource(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            Destroy(source);
            Log("测试音频播放完成，音频源已清理");
        }
    }

    /// <summary>
    /// 测试场景加载
    /// </summary>
    private void TestSceneLoad()
    {
        Log($"开始测试场景加载: {testSceneName}");

        ResourceManager.LoadSceneAsync(testSceneName, () =>
        {
            Log($"✓ 场景加载完成: {testSceneName}");
        });
    }

    /// <summary>
    /// 测试清除缓存
    /// </summary>
    private void TestClearCache()
    {
        Log("开始测试清除缓存...");

        ResourceManager.ClearCache();

        Log("✓ 缓存已清除");
        Log("✓ 未使用的资源已卸载");
        Log("✓ 垃圾回收已执行");
    }

    /// <summary>
    /// 加载进度事件回调
    /// </summary>
    private void OnLoadProgress(float progress)
    {
        // 只在日志中显示关键进度点
        int percentage = Mathf.RoundToInt(progress * 100);
        if (percentage % 25 == 0)
        {
            Log($"加载进度: {percentage}%");
        }
    }

    /// <summary>
    /// 加载信息事件回调
    /// </summary>
    private void OnLoadInfoChanged(string info)
    {
        Log($"加载信息: {info}");
    }

    /// <summary>
    /// 预加载完成事件回调
    /// </summary>
    private void OnPreloadComplete()
    {
        Log("✓ 预加载完成！");
    }

    /// <summary>
    /// 记录日志
    /// </summary>
    private void Log(string message)
    {
        logCount++;
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] [{logCount}] {message}";

        Debug.Log(logMessage);

        if (logText != null)
        {
            logText.text = logMessage + "\n" + logText.text;

            // 限制日志行数
            string[] lines = logText.text.Split('\n');
            if (lines.Length > maxLogLines)
            {
                string[] limitedLines = new string[maxLogLines];
                System.Array.Copy(lines, limitedLines, maxLogLines);
                logText.text = string.Join("\n", limitedLines);
            }
        }
    }

    /// <summary>
    /// 清空日志
    /// </summary>
    public void ClearLog()
    {
        logCount = 0;
        if (logText != null)
        {
            logText.text = "日志已清空\n";
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (Game.Instance != null)
        {
            Game.Instance.OnLoadProgress -= OnLoadProgress;
            Game.Instance.OnLoadInfoChanged -= OnLoadInfoChanged;
            Game.Instance.OnPreloadComplete -= OnPreloadComplete;
        }
    }
}
