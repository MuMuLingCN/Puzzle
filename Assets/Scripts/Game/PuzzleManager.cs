using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum PuzzleDifficulty {
    Easy,    // 简单 - 3x3
    Normal,  // 复杂 - 5x5
    Hard     // 困难 - 7x7
}

public class PuzzleManager : MonoBehaviour {
    [Header("拼图设置")]
    public PuzzleDifficulty difficulty = PuzzleDifficulty.Easy; // 难度等级
    public Transform puzzleContainer; // 拼图容器
    
    private int rows;
    private int columns;
    private List<GameObject> puzzlePieces = new List<GameObject>();
    public Sprite originalImage;

    private void Start() {
        InitializePuzzle();
    }

    /// <summary>
    /// 初始化拼图
    /// </summary>
    public void InitializePuzzle() {
        // 根据难度设置行列数
        SetDifficulty();
        // 清空容器
        ClearPuzzle();
        // 创建拼图块
        CreatePuzzlePieces();
    }

    /// <summary>
    /// 根据难度等级设置行列数
    /// </summary>
    private void SetDifficulty() {
        switch (difficulty) {
            case PuzzleDifficulty.Easy:
                rows = 3;
                columns = 3;
                break;
            case PuzzleDifficulty.Normal:
                rows = 5;
                columns = 5;
                break;
            case PuzzleDifficulty.Hard:
                rows = 7;
                columns = 7;
                break;
        }
        Debug.Log($"设置难度: {difficulty}, 拼图大小: {rows}x{columns}");
    }

    /// <summary>
    /// 清空拼图
    /// </summary>
    private void ClearPuzzle() {
        foreach (GameObject piece in puzzlePieces) {
            if (piece != null)  Destroy(piece);
        }
        puzzlePieces.Clear();
    }

    /// <summary>
    /// 创建拼图块
    /// </summary>
    private void CreatePuzzlePieces() {
        /// 计算每个拼图块的大小
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();
        Debug.Log($"容器大小: ({containerRect.rect.width}x{containerRect.rect.height})");
        int w = (int)containerRect.rect.width/columns;
        int h = (int)containerRect.rect.height/rows;
        Debug.Log($"每个拼图块的大小: ({w}x{h})");
        /// 计算拼图图块的大小
        int imgw = originalImage.texture.width / columns;
        int imgh = originalImage.texture.height / rows;
        // 计算每个拼图块在原始图片中的区域
        Debug.Log($"每个拼图块在原始图片中的区域: ({imgw}x{imgh})");
        
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                // 创建拼图块对象
                GameObject piece = new GameObject($"PuzzlePiece_{row}_{col}");
                piece.transform.SetParent(puzzleContainer);
                RectTransform pieceRect = piece.AddComponent<RectTransform>();
                pieceRect.anchorMin = new Vector2(0, 1);
                pieceRect.anchorMax = new Vector2(0, 1);
                pieceRect.pivot = new Vector2(0, 1);
                pieceRect.sizeDelta = new Vector2(w, h);
                pieceRect.anchoredPosition = new Vector2(w * col, -h * row);
                // 添加 Image 组件
                Image image = piece.AddComponent<Image>();
                
                // 使用原始图片的纹理创建新的 Sprite
                // 计算 UV 坐标
                float uvX = (float)col / columns;
                float uvY = 1f - (float)(row + 1) / rows; // 从顶部开始
                float uvWidth = 1f / columns;
                float uvHeight = 1f / rows;
                
                // 创建新的 Sprite，使用原始纹理但指定不同的 rect
                Rect spriteRect = new Rect(
                    originalImage.texture.width * uvX,
                    originalImage.texture.height * uvY,
                    originalImage.texture.width * uvWidth,
                    originalImage.texture.height * uvHeight
                );
                
                Sprite pieceSprite = Sprite.Create(
                    originalImage.texture,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                
                // 设置图片
                image.sprite = pieceSprite;
                // 设置 Image 组件的保持纵横比模式
                image.preserveAspect = false; // 不保持纵横比，让图片充满
                image.type = Image.Type.Simple; // 简单模式，充满整个区域
                image.fillCenter = true; // 填充中心
                // 添加到列表
                puzzlePieces.Add(piece);
            }
        }
        
        Debug.Log($"已创建 {rows}x{columns} 的拼图，共 {puzzlePieces.Count} 个拼图块");
    }

    /// <summary>
    /// 更新 Grid Layout Group 设置
    /// </summary>
    private void UpdateGridLayout() {
        // 确保容器有 Grid Layout Group 组件
        GridLayoutGroup gridLayout = puzzleContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null) {
            gridLayout = puzzleContainer.gameObject.AddComponent<GridLayoutGroup>();
            Debug.Log("已添加 Grid Layout Group 组件");
        }
        // 获取容器大小
        RectTransform containerRect = puzzleContainer.GetComponent<RectTransform>();
        float w = containerRect.rect.width/columns;
        float h = containerRect.rect.height/rows;
        Debug.Log($"每个拼图块大小: {w}x{h}");
        // 设置 Grid Layout Group
        gridLayout.cellSize = new Vector2(w, h);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// 打乱拼图（可选功能）
    /// </summary>
    public void ShufflePuzzle() {
        // 这里可以添加打乱拼图的逻辑
    }
}
