using UnityEngine;

/// <summary>
/// 场景初始化器，用于设置拼图游戏的初始状态
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("拼图设置")]
    public Sprite puzzleSprite;
    public int gridSize = 5;
    public float puzzleSize = 600f;
    public float pieceSpacing = 2f;

    private void Awake()
    {
        // 确保Game实例存在
        if (Game.Instance == null)
        {
            GameObject gameObj = new GameObject("Game");
            gameObj.AddComponent<Game>();
            Debug.Log("自动创建Game实例");
        }

        // 初始化拼图游戏
        InitializePuzzleGame();
    }

    /// <summary>
    /// 初始化拼图游戏
    /// </summary>
    private void InitializePuzzleGame()
    {
        if (puzzleSprite == null)
        {
            Debug.LogWarning("SceneInitializer: 未设置拼图图片");
            return;
        }

        Debug.Log($"SceneInitializer: 初始化拼图游戏 - 网格大小: {gridSize}x{gridSize}");

        // 这里可以添加更多的初始化逻辑
        // 例如：创建拼图块、设置游戏逻辑等

        // 初始化完成后可以销毁这个初始化器
        // Destroy(gameObject);
    }

    /// <summary>
    /// 在编辑器中显示配置信息
    /// </summary>
    private void OnDrawGizmos()
    {
        if (puzzleSprite != null)
        {
            Gizmos.color = Color.green;
            Vector3 size = new Vector3(puzzleSize, puzzleSize, 0.1f);
            Gizmos.DrawWireCube(transform.position, size);
        }
    }
}
