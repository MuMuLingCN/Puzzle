using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 首页控制器，管理首页UI和交互
/// </summary>
public class HomeController : MonoBehaviour
{
    [Header("UI按钮")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button levelButton;
    [SerializeField] private Button settingsButton;

    [Header("设置面板")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;

    [Header("文本元素")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI versionText;

    private void Start()
    {
        InitializeUI();
        BindEvents();
        UpdateVersionText();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 确保设置面板默认隐藏
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        // 设置标题
        if (titleText != null)
        {
            titleText.text = "拼图游戏";
        }
    }

    /// <summary>
    /// 绑定事件
    /// </summary>
    private void BindEvents()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (levelButton != null)
        {
            levelButton.onClick.AddListener(OnLevelButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);
        }
    }

    /// <summary>
    /// 更新版本号显示
    /// </summary>
    private void UpdateVersionText()
    {
        if (versionText != null)
        {
            versionText.text = $"版本: {Application.version}";
        }
    }

    #region 按钮事件处理

    /// <summary>
    /// 开始游戏按钮点击
    /// </summary>
    private void OnPlayButtonClicked()
    {
        Debug.Log("点击开始游戏按钮");
        StartCoroutine(LoadGameScene());
    }

    /// <summary>
    /// 关卡按钮点击
    /// </summary>
    private void OnLevelButtonClicked()
    {
        Debug.Log("点击关卡按钮");
        StartCoroutine(LoadLevelScene());
    }

    /// <summary>
    /// 设置按钮点击
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        Debug.Log("点击设置按钮");
        ShowSettingsPanel();
    }

    /// <summary>
    /// 关闭设置按钮点击
    /// </summary>
    private void OnCloseSettingsButtonClicked()
    {
        Debug.Log("点击关闭设置按钮");
        HideSettingsPanel();
    }

    #endregion

    #region 场景加载

    /// <summary>
    /// 加载游戏场景
    /// </summary>
    private IEnumerator LoadGameScene()
    {
        if (Game.Instance != null)
        {
            yield return Game.Instance.LoadSceneAsync("Game");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        }
    }

    /// <summary>
    /// 加载关卡场景
    /// </summary>
    private IEnumerator LoadLevelScene()
    {
        if (Game.Instance != null)
        {
            yield return Game.Instance.LoadSceneAsync("Level");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level");
        }
    }

    #endregion

    #region 设置面板控制

    /// <summary>
    /// 显示设置面板
    /// </summary>
    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("设置面板已显示");
        }
        else
        {
            Debug.LogError("设置面板引用为空！");
        }
    }

    /// <summary>
    /// 隐藏设置面板
    /// </summary>
    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Debug.Log("设置面板已隐藏");
        }
        else
        {
            Debug.LogError("设置面板引用为空！");
        }
    }

    /// <summary>
    /// 切换设置面板显示状态
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool currentState = settingsPanel.activeSelf;
            settingsPanel.SetActive(!currentState);
            Debug.Log($"设置面板显示状态已切换: {!currentState}");
        }
    }

    #endregion

    private void OnDestroy()
    {
        // 解绑事件
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }

        if (levelButton != null)
        {
            levelButton.onClick.RemoveListener(OnLevelButtonClicked);
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(OnCloseSettingsButtonClicked);
        }
    }
}
