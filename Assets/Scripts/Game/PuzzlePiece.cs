using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    private Vector2 oriPos;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private PuzzleManager puzzleManager;
    public int correctRow;
    public int correctCol;
    public int curRow;
    public int curCol;

    public void Initialize(int row, int col, Vector2 pos, PuzzleManager manager) {
        correctRow = row;
        correctCol = col;
        oriPos = pos;
        curRow = row;
        curCol = col;
        puzzleManager = manager;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        oriPos = rectTransform.anchoredPosition;
        Debug.Log($"OnPointerDown: {oriPos}");
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            offset = mouseLocalPos - rectTransform.anchoredPosition;
            Debug.Log($"OnPointerDown: {offset}");
        }
        transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData) {
        Vector2 currentPos = rectTransform.anchoredPosition;
        int targetCol = Mathf.RoundToInt(currentPos.x / rectTransform.sizeDelta.x);
        int targetRow = Mathf.RoundToInt(-currentPos.y / rectTransform.sizeDelta.y);
        Debug.Log($"OnPointerUp: {currentPos}, {targetRow}, {targetCol}");
        if (!puzzleManager.IsValidPosition(targetRow, targetCol) ||
            (targetRow == curRow && targetCol == curCol)) {
            rectTransform.anchoredPosition = oriPos;
            return;
        }
        SwapTo(targetRow, targetCol);
    }

    public void OnDrag(PointerEventData eventData) {
        if (rectTransform == null || canvas == null) return;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            Vector2 diff = mouseLocalPos - offset;
            Debug.Log($"OnPointerDown: {mouseLocalPos}");
            MoveTo(diff-oriPos);
        }
    }

    public void SwapTo(int targetRow, int targetCol) {
        PuzzlePiece target = puzzleManager.GetPieceAt(targetRow, targetCol);
        if (target == null || target == this) return;
        Vector2 temp = target.oriPos;
        target.rectTransform.anchoredPosition = oriPos;
        target.oriPos = oriPos;
        target.curRow = curRow;
        target.curCol = curCol;
        rectTransform.anchoredPosition = temp;
        oriPos = temp;
        curRow = targetRow;
        curCol = targetCol;
        puzzleManager.SwapTo(this, target);
    }

    public void MoveTo(Vector2 diff) {
        rectTransform.anchoredPosition = diff + oriPos;
    }
}
