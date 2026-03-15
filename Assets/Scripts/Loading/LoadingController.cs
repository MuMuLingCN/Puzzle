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
    [SerializeField] private bool autoLoadNextScene = false;
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
                UpdateProgress(Game.Instance.LoadProgress);
                UpdateInfo(Game.Instance.CurrentLoadInfo);
            }
            else
            {
                // 如果没有正在加载，直接加载下一个场景
                if (autoLoadNextScene)
                {
                    StartCoroutine(LoadNextScene());
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
        Debug.Log("预加载完成，准备进入游戏");

        // 显示完成信息
        UpdateInfo("加载完成！");

        // 延迟加载下一个场景
        if (autoLoadNextScene)
        {
            StartCoroutine(LoadNextSceneAfterDelay(1f));
        }
    }

    /// <summary>
    /// 加载下一个场景
    /// </summary>
    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(1f);

        if (Game.Instance != null)
        {
            yield return Game.Instance.LoadSceneAsync(nextSceneName);
        }
    }

    /// <summary>
    /// 延迟加载下一个场景
    /// </summary>
    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
