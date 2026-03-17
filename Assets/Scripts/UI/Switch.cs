using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace UI
{
    /// <summary>
    /// iOS风格的Switch开关组件
    /// 具有丝滑的动画过渡效果
    /// </summary>
    [ExecuteInEditMode]
    public class Switch : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform knobRect;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image knobImage;

        [Header("Colors")]
        [SerializeField] private Color onColor = new Color(0.52f, 0.78f, 0.22f); // iOS绿色
        [SerializeField] private Color offColor = new Color(0.85f, 0.85f, 0.85f); // 灰色
        [SerializeField] private Color knobOnColor = Color.white;
        [SerializeField] private Color knobOffColor = Color.white;

        [Header("Settings")]
        [SerializeField] private bool isOn = false;
        [SerializeField] private float animationDuration = 0.25f;
        [SerializeField] private bool isInteractable = true;

        [Header("Knob Shadow Settings")]
        [SerializeField] private bool enableKnobShadow = true;
        [SerializeField] private float shadowOffset = 2f;
        [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.1f);

        // 状态
        private Coroutine animationCoroutine;
        private bool currentVisualState;
        private float backgroundWidth;
        private float knobWidth;
        private float knobPositionRange;
        private float leftKnobPosition; // 左边Knob的X位置
        private float rightKnobPosition; // 右边Knob的X位置

        // 事件
        public System.Action<bool> OnValueChanged;

        /// <summary>
        /// 开关状态
        /// </summary>
        public bool IsOn
        {
            get { return isOn; }
            set
            {
                if (isOn != value)
                {
                    isOn = value;
                    UpdateSwitchState();
                    OnValueChanged?.Invoke(isOn);
                }
            }
        }

        /// <summary>
        /// 是否可交互
        /// </summary>
        public bool IsInteractable
        {
            get { return isInteractable; }
            set
            {
                isInteractable = value;
                UpdateInteractableVisual();
            }
        }

        private void Awake()
        {
            // 如果没有引用，自动查找
            if (backgroundRect == null)
                backgroundRect = GetComponent<RectTransform>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            // 创建子物体
            InitializeKnob();
        }

        private void Start()
        {
            CalculateDimensions();
            SetImmediateVisualState();
        }

        private void InitializeKnob()
        {
            // 如果没有 knob 引用，创建它
            if (knobRect == null || knobImage == null)
            {
                GameObject knobObj = new GameObject("Knob");
                knobObj.transform.SetParent(transform, false);

                knobRect = knobObj.AddComponent<RectTransform>();
                knobImage = knobObj.AddComponent<Image>();

                // 设置圆形图像
                knobImage.type = Image.Type.Simple;

                // 自动配置Knob大小
                AutoConfigureKnobSize();

                // 如果需要阴影，添加阴影组件
                if (enableKnobShadow)
                {
                    var shadow = knobObj.AddComponent<Shadow>();
                    shadow.effectDistance = new Vector2(shadowOffset, -shadowOffset);
                    shadow.effectColor = shadowColor;
                }

                // 创建Canvas渲染器用于圆形遮罩
                var canvasRenderer = knobObj.GetComponent<CanvasRenderer>();
                if (canvasRenderer == null)
                {
                    canvasRenderer = knobObj.AddComponent<CanvasRenderer>();
                }
            }
        }

        /// <summary>
        /// 根据背景大小自动配置Knob的尺寸
        /// </summary>
        private void AutoConfigureKnobSize()
        {
            if (backgroundRect == null || knobRect == null)
                return;

            // 获取背景的尺寸
            float bgWidth = backgroundRect.rect.width;
            float bgHeight = backgroundRect.rect.height;

            // Knob直径 = 背景高度（减去一些内边距）
            float knobSize = bgHeight * 0.875f; // 0.875 = 28/32 (标准比例)

            // 设置Knob尺寸
            knobRect.sizeDelta = new Vector2(knobSize, knobSize);

            // 设置Anchor和Pivot居中
            knobRect.anchorMin = new Vector2(0, 0.5f);
            knobRect.anchorMax = new Vector2(0, 0.5f);
            knobRect.pivot = new Vector2(0.5f, 0.5f);

            // 设置初始位置（左边，留出一些边距）
            float edgeInset = (bgHeight - knobSize) / 2f;
            knobRect.anchoredPosition = new Vector2(edgeInset + knobSize / 2f, 0f);

            Debug.Log($"[Switch] 自动配置Knob: 背景({bgWidth}x{bgHeight}) -> Knob({knobSize}x{knobSize})");
        }

        private void CalculateDimensions()
        {
            if (backgroundRect != null)
            {
                backgroundWidth = backgroundRect.rect.width;

                // 重新计算并配置Knob大小（确保尺寸一致）
                AutoConfigureKnobSize();
            }

            if (knobRect != null)
            {
                float knobSize = knobRect.rect.height;

                // 计算边距
                float edgeInset = (backgroundRect.rect.height - knobSize) * 0.5f;
                if (edgeInset < 0) edgeInset = 0;

                // 计算左右位置的绝对坐标
                leftKnobPosition = edgeInset + knobSize / 2f;
                rightKnobPosition = backgroundWidth - edgeInset - knobSize / 2f;

                // 保存移动距离（从左到右）
                knobPositionRange = rightKnobPosition - leftKnobPosition;

                // 保存Knob宽度
                knobWidth = knobSize;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                CalculateDimensions();
                SetImmediateVisualState();
            }
#endif
        }

        /// <summary>
        /// 当在编辑器中重置组件时
        /// </summary>
        private void Reset()
        {
            // 重置时自动配置Knob大小
            AutoConfigureKnobSize();
        }

        /// <summary>
        /// 处理点击事件
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (isInteractable && Application.isPlaying)
            {
                IsOn = !isOn;
            }
        }

        /// <summary>
        /// 更新开关状态并播放动画
        /// </summary>
        private void UpdateSwitchState()
        {
            // 停止之前的动画
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateSwitch(isOn));
        }

        /// <summary>
        /// 动画协程
        /// </summary>
        private IEnumerator AnimateSwitch(bool targetState)
        {
            if (backgroundRect == null || knobRect == null)
                yield break;

            float elapsedTime = 0f;
            float startKnobX = knobRect.anchoredPosition.x;
            float targetKnobX = targetState ? rightKnobPosition : leftKnobPosition;

            Color startBgColor = backgroundImage.color;
            Color targetBgColor = targetState ? onColor : offColor;

            Color startKnobColor = knobImage.color;
            Color targetKnobColor = targetState ? knobOnColor : knobOffColor;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);

                // 使用缓动函数使动画更丝滑
                float easedT = EaseInOutCubic(t);

                // 更新旋钮位置
                float currentKnobX = Mathf.Lerp(startKnobX, targetKnobX, easedT);
                knobRect.anchoredPosition = new Vector2(currentKnobX, 0f);

                // 更新背景颜色
                backgroundImage.color = Color.Lerp(startBgColor, targetBgColor, easedT);

                // 更新旋钮颜色
                knobImage.color = Color.Lerp(startKnobColor, targetKnobColor, easedT);

                yield return null;
            }

            // 确保最终状态精确
            knobRect.anchoredPosition = new Vector2(targetKnobX, 0f);
            backgroundImage.color = targetBgColor;
            knobImage.color = targetKnobColor;

            currentVisualState = targetState;
        }

        /// <summary>
        /// 立即设置视觉状态（无动画）
        /// </summary>
        private void SetImmediateVisualState()
        {
            if (backgroundRect == null || knobRect == null)
                return;

            float targetKnobX = isOn ? rightKnobPosition : leftKnobPosition;
            Color targetBgColor = isOn ? onColor : offColor;
            Color targetKnobColor = isOn ? knobOnColor : knobOffColor;

            knobRect.anchoredPosition = new Vector2(targetKnobX, 0f);
            backgroundImage.color = targetBgColor;
            knobImage.color = targetKnobColor;

            currentVisualState = isOn;
            UpdateInteractableVisual();
        }

        /// <summary>
        /// 更新可交互状态的视觉效果
        /// </summary>
        private void UpdateInteractableVisual()
        {
            if (backgroundImage != null)
            {
                Color color = backgroundImage.color;
                color.a = isInteractable ? 1f : 0.5f;
                backgroundImage.color = color;
            }

            if (knobImage != null)
            {
                Color color = knobImage.color;
                color.a = isInteractable ? 1f : 0.5f;
                knobImage.color = color;
            }
        }

        /// <summary>
        /// 缓动函数：EaseInOutCubic
        /// 提供丝滑的加速和减速效果
        /// </summary>
        private float EaseInOutCubic(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        /// <summary>
        /// 在编辑器中绘制调试信息
        /// </summary>
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (backgroundRect != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                Gizmos.DrawWireCube(backgroundRect.rect.center, backgroundRect.rect.size);
            }
        }
#endif

        /// <summary>
        /// 设置自定义颜色
        /// </summary>
        public void SetColors(Color newOnColor, Color newOffColor, Color newKnobColor)
        {
            onColor = newOnColor;
            offColor = newOffColor;
            knobOnColor = newKnobColor;
            knobOffColor = newKnobColor;
            SetImmediateVisualState();
        }

        /// <summary>
        /// 设置动画持续时间
        /// </summary>
        public void SetAnimationDuration(float duration)
        {
            animationDuration = Mathf.Max(0.01f, duration);
        }

        /// <summary>
        /// 重置为默认设置
        /// </summary>
        [ContextMenu("Reset to Default")]
        public void ResetToDefault()
        {
            isOn = false;
            onColor = new Color(0.52f, 0.78f, 0.22f);
            offColor = new Color(0.85f, 0.85f, 0.85f);
            knobOnColor = Color.white;
            knobOffColor = Color.white;
            animationDuration = 0.25f;
            isInteractable = true;
            SetImmediateVisualState();
        }
    }
}
