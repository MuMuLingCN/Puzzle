using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public static class ImageExtensions {
    public static void AddRoundedCornersAndBorder(this Image image, Color borderColor, float borderWidth) {
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
        image.transform.SetParent(maskObj.transform);
        image.transform.localPosition = Vector3.zero;
        image.transform.localScale = Vector3.one;
        Outline outline = maskObj.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(borderWidth, borderWidth);
    }
}

public enum PuzzleDifficulty {
    Easy,
    Normal,
    Hard
}

public class PuzzleManager : MonoBehaviour {
    [Header("拼图设置")]
    public PuzzleDifficulty difficulty = PuzzleDifficulty.Easy;
    public Transform puzzleContainer;
    public GameObject setting;
    public int rows;
    public int columns;
    public List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();
    public Sprite originalImage;
    public float pieceWidth;
    public float pieceHeight;
    private RectTransform containerRect;

    private void Start() {
        InitializePuzzle();
        Button settingButton = setting.GetComponent<Button>();
        if (settingButton != null) {
            settingButton.onClick.AddListener(ResetPuzzle);
        }
    }

    public void InitializePuzzle() {
        SetDifficulty();
        ResetPuzzle();
    }

    public void ResetPuzzle() {
        ClearPuzzle();
        CreatePuzzlePieces();
        ShufflePuzzle();
    }

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

    private void ClearPuzzle() {
        foreach (PuzzlePiece piece in puzzlePieces) {
            if (piece.gameObject != null) Destroy(piece.gameObject);
        }
        puzzlePieces.Clear();
    }

    private void CreatePuzzlePieces() {
        containerRect = puzzleContainer.GetComponent<RectTransform>();
        pieceWidth = containerRect.rect.width / columns;
        pieceHeight = containerRect.rect.height / rows;
        for (int row = 0; row < rows; row++) {
            for (int col = 0; col < columns; col++) {
                GameObject piece = new GameObject($"PuzzlePiece_{row}_{col}");
                piece.transform.SetParent(puzzleContainer);
                RectTransform pieceRect = piece.AddComponent<RectTransform>();
                Vector2 pos = new Vector2(pieceWidth * col, -pieceHeight * row);
                pieceRect.anchorMin = new Vector2(0, 1);
                pieceRect.anchorMax = new Vector2(0, 1);
                pieceRect.pivot = new Vector2(0, 1);
                pieceRect.localScale = Vector3.one;
                pieceRect.sizeDelta = new Vector2(pieceWidth, pieceHeight);
                pieceRect.anchoredPosition = pos;
                Image image = piece.AddComponent<Image>();
                PuzzlePiece puzzlePiece = piece.AddComponent<PuzzlePiece>();
                puzzlePiece.Initialize(row, col, pos, this);
                float uvX = (float)col / columns;
                float uvY = 1f - (float)(row + 1) / rows;
                float uvWidth = 1f / columns;
                float uvHeight = 1f / rows;
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
                image.sprite = pieceSprite;
                puzzlePieces.Add(puzzlePiece);
            }
        }
    }

    public void ShufflePuzzle() {
        if (puzzlePieces.Count == 0) return;
        int n = puzzlePieces.Count;
        System.Random random = new System.Random();
        while (n > 1) {
            n--;
            int k = random.Next(n + 1);
            PuzzlePiece piece = puzzlePieces[n];
            piece.SwapTo(k / columns, k % columns);
        }
    }

    public bool IsValidPosition(int row, int col) {
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }

    public void SwapTo(PuzzlePiece piece, PuzzlePiece target) {
        int n = piece.curRow * columns + piece.curCol;
        int k = target.curRow * columns + target.curCol;
        PuzzlePiece temp = puzzlePieces[n];
        puzzlePieces[n] = puzzlePieces[k];
        puzzlePieces[k] = temp;
    }

    public PuzzlePiece GetPieceAt(int row, int col) {
        int idx = row * columns + col;
        PuzzlePiece piece = puzzlePieces[idx];
        return piece;
    }
}
