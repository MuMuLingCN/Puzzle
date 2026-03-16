using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

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
    private List<PuzzlePiece> neighbors = new List<PuzzlePiece>();
    private bool isAnimating = false;

    public void Initialize(int row, int col, Vector2 pos, PuzzleManager manager) {
        correctRow = row;
        correctCol = col;
        oriPos = pos;
        curRow = row;
        curCol = col;
        puzzleManager = manager;
        neighbors = new List<PuzzlePiece>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (isAnimating) return;
        oriPos = rectTransform.anchoredPosition;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            offset = mouseLocalPos - rectTransform.anchoredPosition;
        }
        neighbors.Clear();
        GetNeighbors(neighbors);
        if (!neighbors.Contains(this)) neighbors.Add(this);
        Debug.Log($"邻居数量: {neighbors.Count}");
        neighbors.Sort((a, b) => (a.curRow*puzzleManager.columns+a.curCol).CompareTo(b.curRow*puzzleManager.columns+b.curCol));
        transform.SetAsLastSibling();
        for (int i = 0; i < neighbors.Count; i++) {
            neighbors[i].transform.SetAsLastSibling();
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (isAnimating) return;
        Vector2 currentPos = rectTransform.anchoredPosition;
        int targetCol = Mathf.RoundToInt(currentPos.x / rectTransform.sizeDelta.x);
        int targetRow = Mathf.RoundToInt(-currentPos.y / rectTransform.sizeDelta.y);
        if (!CanSwap(targetRow, targetCol)) {
            for (int i = 0; i < neighbors.Count; i++) {
                // neighbors[i].rectTransform.anchoredPosition = neighbors[i].oriPos;
                StartCoroutine(neighbors[i].AnimateSwap(neighbors[i].oriPos,neighbors[i].rectTransform.anchoredPosition));
            }
            return;
        }
        SwapNeighbors(targetRow, targetCol);
    }

    public void OnDrag(PointerEventData eventData) {
        if (isAnimating || rectTransform == null || canvas == null) return;
        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, canvas.worldCamera, out Vector2 mouseLocalPos)) {
            Vector2 diff = mouseLocalPos - offset;
            MoveTo(diff-oriPos);
            foreach (PuzzlePiece neighbor in neighbors) {
                neighbor.MoveTo(diff-oriPos);
            }
        }
    }

    public void SwapTo(int targetRow, int targetCol) {
        PuzzlePiece target = puzzleManager.GetPieceAt(targetRow, targetCol);
        if (target == null || target == this) return;
        
        Vector2 myStartPos = rectTransform.anchoredPosition;
        Vector2 targetStartPos = target.rectTransform.anchoredPosition;
        Vector2 myTargetPos = target.oriPos;
        Vector2 targetTargetPos = oriPos;
        
        StartCoroutine(AnimateSwap(myTargetPos, myStartPos));
        StartCoroutine(target.AnimateSwap(targetTargetPos, targetStartPos));
        
        target.oriPos = targetTargetPos;
        target.curRow = curRow;
        target.curCol = curCol;
        oriPos = myTargetPos;
        curRow = targetRow;
        curCol = targetCol;
        puzzleManager.SwapTo(this, target);
    }

    private System.Collections.IEnumerator AnimateSwap(Vector2 tarPos, Vector2 starPos) {
      isAnimating = true;
      float duration = 0.3f;
      float elapsed = 0f;
      while (elapsed < duration) {
          float t = elapsed / duration;
          t = EaseOutCubic(t);
          rectTransform.anchoredPosition = Vector2.Lerp(starPos, tarPos, t);
          elapsed += Time.deltaTime;
          yield return null;
      }
      rectTransform.anchoredPosition = tarPos;
      isAnimating = false;
    }

    private float EaseOutCubic(float t) {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    public void ForceSwapTo(int trow, int tcol) {
        rectTransform.anchoredPosition = new Vector2(tcol * puzzleManager.pieceWidth, -trow * puzzleManager.pieceHeight);
        oriPos = rectTransform.anchoredPosition;
        curRow = trow;
        curCol = tcol;
        puzzleManager.puzzlePieces[curRow * puzzleManager.columns + curCol] = this;
    }

    public void MoveTo(Vector2 diff) {
        rectTransform.anchoredPosition = diff + oriPos;
    }

    public bool ValidNeighbor(List<PuzzlePiece> allPieces, PuzzlePiece tar) {
        return !allPieces.Contains(tar);
    }

    public void GetNeighbors(List<PuzzlePiece> allPieces) {
        if (curRow > 0) {
          PuzzlePiece up = puzzleManager.GetPieceAt(curRow - 1, curCol);
          if (ValidNeighbor(allPieces, up) && up.correctCol == correctCol && up.correctRow == correctRow - 1) {
            allPieces.Add(up);
            up.GetNeighbors(allPieces);
          }
        }
        if (curRow < puzzleManager.rows - 1) {
          PuzzlePiece down = puzzleManager.GetPieceAt(curRow + 1, curCol);
          if (ValidNeighbor(allPieces, down) && down.correctCol == correctCol && down.correctRow == correctRow + 1) {
            allPieces.Add(down);
            down.GetNeighbors(allPieces);
          }
        }
        if (curCol > 0) {
          PuzzlePiece left = puzzleManager.GetPieceAt(curRow, curCol - 1);
          if (ValidNeighbor(allPieces, left) && left.correctCol == correctCol - 1 && left.correctRow == correctRow) {
            allPieces.Add(left);
            left.GetNeighbors(allPieces);
          }
        }
        if (curCol < puzzleManager.columns - 1) {
          PuzzlePiece right = puzzleManager.GetPieceAt(curRow, curCol + 1);
          if (ValidNeighbor(allPieces, right) && right.correctCol == correctCol + 1 && right.correctRow == correctRow) {
            allPieces.Add(right);
            right.GetNeighbors(allPieces);
          }
        }
    }

    public bool CanSwap(int trow, int tcol) {
        if (!puzzleManager.IsValidPosition(trow, tcol)) return false;
        if (trow == curRow && tcol == curCol) return false;
        for (int i = 0; i < neighbors.Count; i++) {
            int nr = neighbors[i].curRow - curRow + trow;
            int nc = neighbors[i].curCol - curCol + tcol;
            if (nr < 0 || nr >= puzzleManager.rows || nc < 0 || nc >= puzzleManager.columns) {
                return false;
            }
        }
        return true;
    }

    public void SwapNeighbors(int trow, int tcol) {
        List<PuzzlePiece> temp = new List<PuzzlePiece>(neighbors);
        int offsetRow = trow - curRow;
        int offsetCol = tcol - curCol;
        if (trow > curRow || (trow == curRow && tcol > curCol)) {
            temp.Reverse();
        }
        for (int i = 0; i < temp.Count; i++) {
            int nr = temp[i].curRow + offsetRow;
            int nc = temp[i].curCol + offsetCol;
            temp[i].SwapTo(nr, nc);
        }
    }
}
