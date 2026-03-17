using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Modal（弹窗）控制类
/// 实现弹窗的打开和关闭，并支持蒙版动画和打开动画效果
/// </summary>
public class Modal : MonoBehaviour
{
    [Header("UI 组件")]
    [Tooltip("蒙版背景")]
    public Image maskImage;

    [Tooltip("弹窗内容面板")]
    public RectTransform modalPanel;

    [Header("动画设置")]
    [Tooltip("弹窗打开动画时长")]
    public float openDuration = 0.3f;

    [Tooltip("弹窗关闭动画时长")]
    public float closeDuration = 0.2f;

    [Tooltip("蒙版目标透明度")]
    public float maskTargetAlpha = 0.6f;

    [Tooltip("弹窗打开时的缩放比例")]
    public float openScale = 1.0f;

    [Tooltip("弹窗打开动画曲线（弹跳效果）- 用于控制缩放比例")]
    public AnimationCurve openAnimationCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 3f),
        new Keyframe(0.5f, 1.2f, 0f, 0f),
        new Keyframe(0.8f, 0.95f, 0f, 0f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [Tooltip("弹窗关闭动画曲线（平滑缩小）- 用于控制缩放比例")]
    public AnimationCurve closeAnimationCurve = new AnimationCurve(
        new Keyframe(0f, 1f, -1f, -1f),
        new Keyframe(1f, 0f, -1f, -1f)
    );

    [Header("交互设置")]
    [Tooltip("是否允许点击蒙版关闭弹窗")]
    public bool clickMaskToClose = true;

    [Tooltip("弹窗初始状态（是否默认打开）")]
    public bool openOnStart = true;

    [Header("音效设置")]
    [Tooltip("打开弹窗音效名称")]
    public string openSoundName;

    [Tooltip("关闭弹窗音效名称")]
    public string closeSoundName;

    [Tooltip("音效音量")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine currentAnimationCoroutine;

    private void Awake()
    {
        // 初始化状态
        if (maskImage != null)
        {
            maskImage.gameObject.SetActive(false);
            Color maskColor = maskImage.color;
            maskColor.a = 0f;
            maskImage.color = maskColor;
        }

        if (modalPanel != null)
        {
            modalPanel.gameObject.SetActive(false);
            modalPanel.localScale = Vector3.zero;
        }
    }

    private void Start()
    {
        // 如果允许点击蒙版关闭，为蒙版添加点击事件
        if (clickMaskToClose && maskImage != null)
        {
            Button maskButton = maskImage.gameObject.GetComponent<Button>();
            if (maskButton == null)
            {
                maskButton = maskImage.gameObject.AddComponent<Button>();
            }
            maskButton.onClick.AddListener(OnMaskClick);
        }

        // 如果默认打开，则打开弹窗（使用动画）
        if (openOnStart)
        {
            // 延迟一帧执行，确保所有组件都已初始化
            StartCoroutine(DelayedOpen());
        }
    }

    /// <summary>
    /// 延迟打开弹窗（确保组件初始化完成）
    /// </summary>
    private IEnumerator DelayedOpen()
    {
        yield return null; // 等待一帧
        Open();
    }

    /// <summary>
    /// 蒙版点击事件处理
    /// </summary>
    private void OnMaskClick()
    {
        if (clickMaskToClose && isOpen)
        {
            Close();
        }
    }

    /// <summary>
    /// 打开弹窗
    /// </summary>
    public void Open()
    {
        if (isOpen || isAnimating)
            return;

        // 播放打开音效
        if (!string.IsNullOrEmpty(openSoundName) && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(openSoundName, soundVolume);
        }

        // 启动打开动画
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(OpenAnimation());
    }

    /// <summary>
    /// 关闭弹窗
    /// </summary>
    public void Close()
    {
        if (!isOpen || isAnimating)
            return;

        // 播放关闭音效
        if (!string.IsNullOrEmpty(closeSoundName) && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(closeSoundName, soundVolume);
        }

        // 启动关闭动画
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(CloseAnimation());
    }

    /// <summary>
    /// 切换弹窗状态
    /// </summary>
    public void Toggle()
    {
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// 打开动画协程
    /// </summary>
    private IEnumerator OpenAnimation()
    {
        isAnimating = true;

        // 激活游戏对象
        if (maskImage != null) maskImage.gameObject.SetActive(true);
        if (modalPanel != null) modalPanel.gameObject.SetActive(true);

        float elapsedTime = 0f;

        // 设置初始状态
        if (modalPanel != null)
        {
            modalPanel.localScale = Vector3.zero;
        }
        if (maskImage != null)
        {
            Color maskColor = maskImage.color;
            maskColor.a = 0f;
            maskImage.color = maskColor;
        }

        // 执行动画
        while (elapsedTime < openDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / openDuration;
            float curveValue = openAnimationCurve.Evaluate(progress);

            // 蒙版淡入动画（使用进度而非曲线值）
            if (maskImage != null)
            {
                Color maskColor = maskImage.color;
                maskColor.a = Mathf.Lerp(0f, maskTargetAlpha, progress);
                maskImage.color = maskColor;
            }

            // 弹窗缩放动画（直接使用曲线值作为缩放比例）
            if (modalPanel != null)
            {
                modalPanel.localScale = Vector3.one * openScale * curveValue;
            }

            yield return null;
        }

        // 确保最终状态正确
        if (maskImage != null)
        {
            Color maskColor = maskImage.color;
            maskColor.a = maskTargetAlpha;
            maskImage.color = maskColor;
        }
        if (modalPanel != null)
        {
            modalPanel.localScale = Vector3.one * openScale;
        }

        isOpen = true;
        isAnimating = false;
        currentAnimationCoroutine = null;
    }

    /// <summary>
    /// 关闭动画协程
    /// </summary>
    private IEnumerator CloseAnimation()
    {
        isAnimating = true;

        float elapsedTime = 0f;

        // 执行动画
        while (elapsedTime < closeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / closeDuration;
            float curveValue = closeAnimationCurve.Evaluate(progress);

            // 蒙版淡出动画（使用进度）
            if (maskImage != null)
            {
                Color maskColor = maskImage.color;
                maskColor.a = Mathf.Lerp(maskTargetAlpha, 0f, progress);
                maskImage.color = maskColor;
            }

            // 弹窗缩放动画（直接使用曲线值作为缩放比例）
            if (modalPanel != null)
            {
                modalPanel.localScale = Vector3.one * openScale * curveValue;
            }

            yield return null;
        }

        // 确保最终状态正确
        if (modalPanel != null)
        {
            modalPanel.localScale = Vector3.zero;
        }

        // 隐藏游戏对象
        if (maskImage != null) maskImage.gameObject.SetActive(false);
        if (modalPanel != null) modalPanel.gameObject.SetActive(false);

        isOpen = false;
        isAnimating = false;
        currentAnimationCoroutine = null;
    }

    /// <summary>
    /// 判断弹窗是否打开
    /// </summary>
    public bool IsOpen => isOpen;

    /// <summary>
    /// 判断是否正在播放动画
    /// </summary>
    public bool IsAnimating => isAnimating;

    /// <summary>
    /// 立即打开弹窗（无动画）
    /// </summary>
    public void OpenImmediate()
    {
        if (isOpen)
            return;

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }

        isAnimating = false;

        if (maskImage != null)
        {
            maskImage.gameObject.SetActive(true);
            Color maskColor = maskImage.color;
            maskColor.a = maskTargetAlpha;
            maskImage.color = maskColor;
        }

        if (modalPanel != null)
        {
            modalPanel.gameObject.SetActive(true);
            modalPanel.localScale = Vector3.one * openScale;
        }

        isOpen = true;
    }

    /// <summary>
    /// 立即关闭弹窗（无动画）
    /// </summary>
    public void CloseImmediate()
    {
        if (!isOpen)
            return;

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }

        isAnimating = false;

        if (maskImage != null)
        {
            maskImage.gameObject.SetActive(false);
            Color maskColor = maskImage.color;
            maskColor.a = 0f;
            maskImage.color = maskColor;
        }

        if (modalPanel != null)
        {
            modalPanel.gameObject.SetActive(false);
            modalPanel.localScale = Vector3.zero;
        }

        isOpen = false;
    }

    private void OnDestroy()
    {
        // 清理协程
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }

        // 清理蒙版点击事件
        if (clickMaskToClose && maskImage != null)
        {
            Button maskButton = maskImage.gameObject.GetComponent<Button>();
            if (maskButton != null)
            {
                maskButton.onClick.RemoveListener(OnMaskClick);
            }
        }
    }
}
