using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Vector2 originalPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 offset;
    private PuzzlePiece targetPiece;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        originalPosition = rectTransform.anchoredPosition;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            offset = mouseLocalPos - rectTransform.anchoredPosition;
        }
        transform.SetAsLastSibling();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (targetPiece != null) {
            Vector2 targetPosition = targetPiece.rectTransform.anchoredPosition;
            targetPiece.rectTransform.anchoredPosition = originalPosition;
            rectTransform.anchoredPosition = targetPosition;
            targetPiece = null;
        } else {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    public void OnDrag(PointerEventData eventData) {
        if (rectTransform == null || canvas == null) return;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            rectTransform.anchoredPosition = mouseLocalPos - offset;
        }
        targetPiece = FindTargetPiece(eventData.position);
    }

    private PuzzlePiece FindTargetPiece(Vector2 screenPosition) {
        PuzzlePiece[] allPieces = GetComponentsInParent<PuzzleManager>()[0].GetComponentsInChildren<PuzzlePiece>();
        foreach (PuzzlePiece piece in allPieces) {
            if (piece == this) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                piece.rectTransform, screenPosition, canvas.worldCamera)) {
                return piece;
            }
        }
        return null;
    }
}
