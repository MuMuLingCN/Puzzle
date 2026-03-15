using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 加载场景控制器，负责显示加载进度和信息
/// </summary>
public class LoadingController : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("加载设置")]
    [SerializeField] private bool autoLoadNextScene = true;
    [SerializeField] private string nextSceneName = "Home";

    private void Start()
    {
        // 订阅Game实例的加载事件
        if (Game.Instance != null)
        {
            Game.Instance.OnLoadProgress += UpdateProgress;
            Game.Instance.OnLoadInfoChanged += UpdateInfo;
            Game.Instance.OnPreloadComplete += OnPreloadComplete;

            // 显示版本号
            UpdateVersion();

            // 如果游戏正在加载，显示当前进度
            if (Game.Instance.IsLoading)
            {
                Debug.Log("LoadingController: Game正在加载中，等待预加载完成");
                UpdateProgress(Game.Instance.LoadProgress);
                UpdateInfo(Game.Instance.CurrentLoadInfo);
            }
            else
            {
                Debug.Log($"LoadingController: Game没有在加载中，autoLoadNextScene={autoLoadNextScene}");
                // 如果没有正在加载，直接加载下一个场景
                if (autoLoadNextScene)
                {
                    Debug.Log($"LoadingController: 开始加载下一个场景: {nextSceneName}");
                    StartCoroutine(LoadNextScene());
                }
                else
                {
                    Debug.Log("LoadingController: autoLoadNextScene为false，不加载下一个场景");
                }
            }
        }
        else
        {
            Debug.LogError("Game实例未找到！");
        }
    }

    /// <summary>
    /// 更新加载进度条
    /// </summary>
    private void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        if (progressText != null)
        {
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }

    /// <summary>
    /// 更新加载信息文本
    /// </summary>
    private void UpdateInfo(string info)
    {
        if (infoText != null)
        {
            infoText.text = info;
        }
    }

    /// <summary>
    /// 更新版本号显示
    /// </summary>
    private void UpdateVersion()
    {
        if (versionText != null)
        {
            versionText.text = Application.version;
        }
    }

    /// <summary>
    /// 预加载完成回调
    /// </summary>
    private void OnPreloadComplete()
    {
        Debug.Log("LoadingController: 预加载完成，准备进入游戏");

        // 显示完成信息
        UpdateInfo("加载完成！");

        // 延迟加载下一个场景
        if (autoLoadNextScene)
        {
            Debug.Log($"LoadingController: 启动延迟加载，1秒后加载场景: {nextSceneName}");
            StartCoroutine(LoadNextSceneAfterDelay(1f));
        }
        else
        {
            Debug.Log("LoadingController: autoLoadNextScene为false，不加载下一个场景");
        }
    }

    /// <summary>
    /// 加载下一个场景
    /// </summary>
    private IEnumerator LoadNextScene()
    {
        Debug.Log("LoadingController: LoadNextScene协程开始执行");
        yield return new WaitForSeconds(1f);

        Debug.Log($"LoadingController: 准备加载场景: {nextSceneName}");

        if (Game.Instance != null)
        {
            // 简化处理：所有场景都使用普通的异步加载方式
            // 这样可以避免预加载可能带来的问题
            Debug.Log($"LoadingController: 开始加载场景: {nextSceneName}");
            yield return Game.Instance.LoadSceneAsync(nextSceneName);
            Debug.Log("LoadingController: 场景加载完成");
        }
        else
        {
            Debug.LogError("LoadingController: Game.Instance为null，无法加载场景");
        }
    }

    /// <summary>
    /// 延迟加载下一个场景
    /// </summary>
    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        Debug.Log($"LoadingController: LoadNextSceneAfterDelay协程开始，延迟{delay}秒");
        yield return new WaitForSeconds(delay);
        Debug.Log("LoadingController: 延迟结束，调用LoadNextScene");
        yield return LoadNextScene();
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (Game.Instance != null)
        {
            Game.Instance.OnLoadProgress -= UpdateProgress;
            Game.Instance.OnLoadInfoChanged -= UpdateInfo;
            Game.Instance.OnPreloadComplete -= OnPreloadComplete;
        }
    }
}
