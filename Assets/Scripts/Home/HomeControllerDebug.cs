using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// HomeController的调试助手，帮助排查点击事件问题
/// </summary>
public class HomeControllerDebug : MonoBehaviour
{
    [Header("调试信息")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private TextMeshProUGUI debugText;

    private HomeController homeController;

    private void Start()
    {
        homeController = GetComponent<HomeController>();
        RunDiagnostics();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("按下空格键，重新运行诊断");
            RunDiagnostics();
        }
    }

    /// <summary>
    /// 运行诊断检查
    /// </summary>
    private void RunDiagnostics()
    {
        Debug.Log("=== HomeController 诊断开始 ===");

        // 1. 检查HomeController组件
        CheckHomeControllerComponent();

        // 2. 检查UI按钮引用
        CheckButtonReferences();

        // 3. 检查Button组件
        CheckButtonComponents();

        // 4. 检查EventSystem
        CheckEventSystem();

        // 5. 检查Canvas
        CheckCanvas();

        Debug.Log("=== HomeController 诊断完成 ===");

        if (showDebugInfo && debugText != null)
        {
            UpdateDebugText();
        }
    }

    /// <summary>
    /// 检查HomeController组件
    /// </summary>
    private void CheckHomeControllerComponent()
    {
        if (homeController == null)
        {
            Debug.LogError("❌ HomeController组件未找到！");
        }
        else
        {
            Debug.Log("✅ HomeController组件已找到");
        }
    }

    /// <summary>
    /// 检查按钮引用
    /// </summary>
    private void CheckButtonReferences()
    {
        // 使用反射检查私有字段
        var type = typeof(HomeController);
        var fields = type.GetFields(System.Reflection.BindingFlags.NonPublic |
                                  System.Reflection.BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (field.FieldType == typeof(Button))
            {
                var value = field.GetValue(homeController);
                if (value == null)
                {
                    Debug.LogError($"❌ {field.Name} 未连接到UI按钮！");
                }
                else
                {
                    Debug.Log($"✅ {field.Name} 已连接: {(value as Button).name}");
                }
            }
        }
    }

    /// <summary>
    /// 检查Button组件
    /// </summary>
    private void CheckButtonComponents()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);
        Debug.Log($"🔍 场景中找到 {buttons.Length} 个Button组件");

        foreach (var button in buttons)
        {
            if (button.gameObject.activeInHierarchy)
            {
                Debug.Log($"✅ {button.name} 是激活状态");
            }
            else
            {
                Debug.LogWarning($"⚠️ {button.name} 未激活");
            }

            if (button.interactable)
            {
                Debug.Log($"✅ {button.name} 可交互");
            }
            else
            {
                Debug.LogWarning($"⚠️ {button.name} 不可交互");
            }
        }
    }

    /// <summary>
    /// 检查EventSystem
    /// </summary>
    private void CheckEventSystem()
    {
        var eventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystems.Length == 0)
        {
            Debug.LogError("❌ 未找到EventSystem！这是按钮点击不工作的常见原因。");
            Debug.LogError("💡 解决方案：Hierarchy右键 → UI → Event System");
        }
        else
        {
            Debug.Log($"✅ 找到 {eventSystems.Length} 个EventSystem");
        }
    }

    /// <summary>
    /// 检查Canvas
    /// </summary>
    private void CheckCanvas()
    {
        var canvases = FindObjectsOfType<Canvas>();
        if (canvases.Length == 0)
        {
            Debug.LogError("❌ 未找到Canvas！");
        }
        else
        {
            Debug.Log($"✅ 找到 {canvases.Length} 个Canvas");

            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    Debug.Log($"✅ {canvas.name} 使用Screen Space Overlay模式");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {canvas.name} 使用{canvas.renderMode}模式");
                }
            }
        }
    }

    /// <summary>
    /// 更新调试文本
    /// </summary>
    private void UpdateDebugText()
    {
        string info = "调试信息:\n\n";
        info += "按空格键重新运行诊断\n\n";

        info += "检查项:\n";
        info += "✓ HomeController组件\n";
        info += "✓ 按钮引用连接\n";
        info += "✓ 按钮激活状态\n";
        info += "✓ EventSystem存在\n";
        info += "✓ Canvas配置\n\n";

        info += "常见问题:\n";
        info += "1. 按钮未连接到Controller\n";
        info += "2. EventSystem未添加\n";
        info += "3. 按钮被其他UI遮挡\n";
        info += "4. Button的Interactable为false\n";
        info += "5. 按钮GameObject未激活\n";

        debugText.text = info;
    }

    /// <summary>
    /// 手动测试按钮点击
    /// </summary>
    [ContextMenu("测试所有按钮点击")]
    public void TestAllButtonClicks()
    {
        Debug.Log("=== 测试按钮点击 ===");

        var buttons = FindObjectsOfType<Button>(true);
        foreach (var button in buttons)
        {
            Debug.Log($"模拟点击: {button.name}");
            if (button.gameObject.activeInHierarchy)
            {
                button.onClick.Invoke();
            }
            else
            {
                Debug.LogWarning($"按钮 {button.name} 未激活，无法点击");
            }
        }

        Debug.Log("=== 测试完成 ===");
    }
}
