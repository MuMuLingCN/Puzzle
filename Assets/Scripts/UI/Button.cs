using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Button控制辅助组件
    /// 提供音效播放和按压/松开动画效果
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class Button : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Audio Settings")]
        [SerializeField] private bool enableAudio = true;
        [SerializeField] private string pressSoundKey = "";
        [SerializeField] private string releaseSoundKey = "";
        [SerializeField] private string hoverSoundKey = "";

        [Header("Animation Settings")]
        [SerializeField] private bool enableAnimation = true;
        [SerializeField] private float pressScale = 0.95f;
        [SerializeField] private float releaseScale = 1.0f;
        [SerializeField] private float animationSpeed = 10f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        [Header("References")]
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private UnityEngine.UI.Button targetButton;

        // 状态
        private bool isPressed = false;
        private Vector3 originalScale;
        private Coroutine animationCoroutine;

        // 事件
        public System.Action OnPress;
        public System.Action OnRelease;
        public System.Action OnClick;

        /// <summary>
        /// 是否启用音效
        /// </summary>
        public bool EnableAudio
        {
            get { return enableAudio; }
            set { enableAudio = value; }
        }

        /// <summary>
        /// 是否启用动画
        /// </summary>
        public bool EnableAnimation
        {
            get { return enableAnimation; }
            set { enableAnimation = value; }
        }

        private void Awake()
        {
            // 自动获取引用
            if (targetTransform == null)
                targetTransform = GetComponent<RectTransform>();

            if (targetButton == null)
                targetButton = GetComponent<UnityEngine.UI.Button>();

            // 保存原始缩放
            if (targetTransform != null)
            {
                originalScale = targetTransform.localScale;
            }

            // 绑定Button的点击事件
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(HandleButtonClick);
            }
        }

        private void OnDestroy()
        {
            // 解绑事件
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(HandleButtonClick);
            }
        }

        /// <summary>
        /// 处理指针按下事件
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isPressed) return;
            isPressed = true;

            // 播放按下音效
            if (enableAudio && !string.IsNullOrEmpty(pressSoundKey))
            {
                PlaySound(pressSoundKey);
            }

            // 播放按下动画
            if (enableAnimation)
            {
                PlayPressAnimation();
            }

            // 触发按下事件
            OnPress?.Invoke();
        }

        /// <summary>
        /// 处理指针松开事件
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPressed) return;
            isPressed = false;

            // 播放松开音效
            if (enableAudio && !string.IsNullOrEmpty(releaseSoundKey))
            {
                PlaySound(releaseSoundKey);
            }

            // 播放松开动画
            if (enableAnimation)
            {
                PlayReleaseAnimation();
            }

            // 触发松开事件
            OnRelease?.Invoke();
        }

        /// <summary>
        /// 处理按钮点击事件
        /// </summary>
        private void HandleButtonClick()
        {
            // 触发点击事件
            OnClick?.Invoke();
        }

        /// <summary>
        /// 播放按下动画
        /// </summary>
        private void PlayPressAnimation()
        {
            if (targetTransform == null) return;

            // 停止之前的动画
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateToScale(originalScale * pressScale));
        }

        /// <summary>
        /// 播放松开动画
        /// </summary>
        private void PlayReleaseAnimation()
        {
            if (targetTransform == null) return;

            // 停止之前的动画
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(AnimateToScale(originalScale * releaseScale));
        }

        /// <summary>
        /// 动画协程：缩放到目标值
        /// </summary>
        private System.Collections.IEnumerator AnimateToScale(Vector3 targetScale)
        {
            if (targetTransform == null) yield break;

            Vector3 startScale = targetTransform.localScale;
            float elapsedTime = 0f;
            float animationDuration = 1f / animationSpeed;

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                float curveValue = animationCurve.Evaluate(t);

                targetTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

                yield return null;
            }

            // 确保最终状态精确
            targetTransform.localScale = targetScale;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        private void PlaySound(string soundKey)
        {
            if (string.IsNullOrEmpty(soundKey)) return;

            // 尝试获取AudioManager
            var audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlaySound(soundKey);
            }
            else
            {
                Debug.LogWarning($"[Button] AudioManager not found, cannot play sound: {soundKey}");
            }
        }

        /// <summary>
        /// 设置目标Transform（用于动画）
        /// </summary>
        public void SetTargetTransform(RectTransform target)
        {
            targetTransform = target;
            if (targetTransform != null)
            {
                originalScale = targetTransform.localScale;
            }
        }

        /// <summary>
        /// 设置动画参数
        /// </summary>
        public void SetAnimationParameters(float press, float release, float speed)
        {
            pressScale = press;
            releaseScale = release;
            animationSpeed = speed;
        }

        /// <summary>
        /// 设置音效Key
        /// </summary>
        public void SetSoundKeys(string press, string release, string hover)
        {
            pressSoundKey = press;
            releaseSoundKey = release;
            hoverSoundKey = hover;
        }

        /// <summary>
        /// 模拟按下效果（用于代码触发）
        /// </summary>
        public void SimulatePress()
        {
            OnPointerDown(null);
        }

        /// <summary>
        /// 模拟松开效果（用于代码触发）
        /// </summary>
        public void SimulateRelease()
        {
            OnPointerUp(null);
        }

        /// <summary>
        /// 重置为默认设置
        /// </summary>
        [ContextMenu("Reset to Default")]
        public void ResetToDefault()
        {
            enableAudio = true;
            enableAnimation = true;
            pressScale = 0.95f;
            releaseScale = 1.0f;
            animationSpeed = 10f;
            animationCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
            pressSoundKey = "button_press";
            releaseSoundKey = "button_release";
            hoverSoundKey = "button_hover";
        }

        /// <summary>
        /// 在编辑器中绘制调试信息
        /// </summary>
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (targetTransform != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawWireCube(targetTransform.rect.center, targetTransform.rect.size);
            }
        }
#endif
    }
}
