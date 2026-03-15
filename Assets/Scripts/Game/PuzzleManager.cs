using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class ImageExtensions
{
    /// <summary>
    /// 给 Image 添加圆角和边框
    /// </summary>
    /// <param name="image">目标 Image 组件</param>
    /// <param name="borderColor">边框颜色</param>
    /// <param name="borderWidth">边框宽度</param>
    public static void AddRoundedCornersAndBorder(this Image image, Color borderColor, float borderWidth)
    {
        // 创建圆角遮罩
        GameObject maskObj = new GameObject("RoundedMask");
        maskObj.transform.SetParent(image.transform.parent);
        maskObj.transform.localPosition = image.transform.localPosition;
        maskObj.transform.localScale = image.transform.localScale;
        
        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = image.rectTransform.anchorMin;
        maskRect.anchorMax = image.rectTransform.anchorMax;
        maskRect.offsetMin = image.rectTransform.offsetMin;
        maskRect.offsetMax = image.rectTransform.offsetMax;
        
        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.white;
        
        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        // 移动 Image 到遮罩下
        image.transform.SetParent(maskObj.transform);
        image.transform.localPosition = Vector3.zero;
        image.transform.localScale = Vector3.one;
        
        // 添加边框
        Outline outline = maskObj.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(borderWidth, borderWidth);
    }
}

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
        // 随机打乱拼图
        ShufflePuzzle();
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
        
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                // 创建拼图块对象
                GameObject piece = new GameObject($"PuzzlePiece_{row}_{col}");
                piece.transform.SetParent(puzzleContainer);
                RectTransform pieceRect = piece.AddComponent<RectTransform>();
                pieceRect.anchorMin = new Vector2(0, 1);
                pieceRect.anchorMax = new Vector2(0, 1);
                pieceRect.pivot = new Vector2(0, 1);
                pieceRect.localScale = Vector3.one;
                pieceRect.sizeDelta = new Vector2(w, h);
                pieceRect.anchoredPosition = new Vector2(w * col, -h * row);
                // 添加 Image 组件
                Image image = piece.AddComponent<Image>();
                // 添加 PuzzlePiece 组件，实现拖拽功能
                piece.AddComponent<PuzzlePiece>();
                
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
                // image.AddRoundedCornersAndBorder(new Color(0.3f, 0.3f, 0.3f, 1f), 3f);
                puzzlePieces.Add(piece);
            }
        }
        
        Debug.Log($"已创建 {rows}x{columns} 的拼图，共 {puzzlePieces.Count} 个拼图块");
    }

    /// <summary>
    /// 打乱拼图（可选功能）
    /// </summary>
    public void ShufflePuzzle() {
        if (puzzlePieces.Count == 0) return;
        // 保存原始位置
        List<Vector2> originalPositions = new List<Vector2>();
        foreach (GameObject piece in puzzlePieces) {
            if (piece != null) {
                RectTransform rect = piece.GetComponent<RectTransform>();
                if (rect != null) {
                    originalPositions.Add(rect.anchoredPosition);
                }
            }
        }
        // 随机打乱位置
        System.Random random = new System.Random();
        int n = originalPositions.Count;
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            Vector2 temp = originalPositions[k];
            originalPositions[k] = originalPositions[n];
            originalPositions[n] = temp;
        }
        // 应用打乱后的位置
        for (int i = 0; i < puzzlePieces.Count; i++) {
            GameObject piece = puzzlePieces[i];
            if (piece != null) {
                RectTransform rect = piece.GetComponent<RectTransform>();
                if (rect != null && i < originalPositions.Count) {
                    rect.anchoredPosition = originalPositions[i];
                }
            }
        }
        Debug.Log("拼图已打乱");
    }
}
