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
        neighbors.Sort((a, b) => (a.curRow*puzzleManager.columns+a.curCol).CompareTo(b.curRow*puzzleManager.columns+b.curCol));
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

    public PuzzlePiece SwapTo(int targetRow, int targetCol) {
        PuzzlePiece target = puzzleManager.GetPieceAt(targetRow, targetCol);
        if (target == null || target == this) return null;
        target.neighbors.Clear();
        GetNeighbors(target.neighbors);
        
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
        return target;
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
        if (!allPieces.Contains(this)) allPieces.Add(this);
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
        List<PuzzlePiece> all = new List<PuzzlePiece>(neighbors);
        int offsetRow = trow - curRow;
        int offsetCol = tcol - curCol;
        if (trow > curRow || (trow == curRow && tcol > curCol)) {
            temp.Reverse();
        }
        for (int i = 0; i < temp.Count; i++) {
            int nr = temp[i].curRow + offsetRow;
            int nc = temp[i].curCol + offsetCol;
            PuzzlePiece target = temp[i].SwapTo(nr, nc);
            if (target != null && !all.Contains(target))  all.Add(target);
        }
        /// 检查并动画连接
        StartCoroutine(CheckAndAnimateConnection(all));
    }
    
    private System.Collections.IEnumerator CheckAndAnimateConnection(List<PuzzlePiece> changedPieces) {
        yield return new WaitForSeconds(0.4f);
        List<PuzzlePiece> used = new List<PuzzlePiece>();
        List<PuzzlePiece> connectedGroups = new List<PuzzlePiece>();
        Debug.Log($"检查连接: 改变的块数量: {changedPieces.Count}");
        for (int i = 0; i < changedPieces.Count; i++) {
            PuzzlePiece piece = changedPieces[i];
            if (used.Contains(piece)) continue;
            used.Add(piece);
            List<PuzzlePiece> connected = new List<PuzzlePiece>();
            piece.GetNeighbors(connected);
            used.AddRange(connected);
            Debug.Log($"检查连接: 邻居数量: {piece.neighbors.Count}, 连接数量: {connected.Count}");
            if (piece.neighbors.Count != connected.Count && connected.Count > 1) {
                connectedGroups.AddRange(connected);
            }
        }
        StartCoroutine(PlayScaleAnimation(connectedGroups));
    }
    
    private System.Collections.IEnumerator PlayScaleAnimation(List<PuzzlePiece> pieces) {
        float scaleUpDuration = 0.15f;
        float scaleDownDuration = 0.15f;
        float maxScale = 1.02f;
        
        foreach (PuzzlePiece piece in pieces) {
            piece.transform.SetAsLastSibling();
            piece.isAnimating = true;
        }
        
        Vector2 centerPos = Vector2.zero;
        foreach (PuzzlePiece piece in pieces) {
            centerPos += piece.rectTransform.anchoredPosition;
        }
        centerPos /= pieces.Count;
        
        Vector2[] originalOffsets = new Vector2[pieces.Count];
        for (int i = 0; i < pieces.Count; i++) {
            originalOffsets[i] = pieces[i].rectTransform.anchoredPosition - centerPos;
        }
        
        float elapsed = 0f;
        while (elapsed < scaleUpDuration) {
            float t = elapsed / scaleUpDuration;
            t = EaseOutBack(t);
            float scale = 1f + (maxScale - 1f) * t;
            
            for (int i = 0; i < pieces.Count; i++) {
                pieces[i].rectTransform.anchoredPosition = centerPos + originalOffsets[i] * scale;
                pieces[i].rectTransform.localScale = Vector3.one * scale;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        for (int i = 0; i < pieces.Count; i++) {
            pieces[i].rectTransform.anchoredPosition = centerPos + originalOffsets[i] * maxScale;
            pieces[i].rectTransform.localScale = Vector3.one * maxScale;
        }
        
        elapsed = 0f;
        while (elapsed < scaleDownDuration) {
            float t = elapsed / scaleDownDuration;
            t = EaseOutCubic(t);
            float scale = maxScale - (maxScale - 1f) * t;
            
            for (int i = 0; i < pieces.Count; i++) {
                pieces[i].rectTransform.anchoredPosition = centerPos + originalOffsets[i] * scale;
                pieces[i].rectTransform.localScale = Vector3.one * scale;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        for (int i = 0; i < pieces.Count; i++) {
            pieces[i].rectTransform.anchoredPosition = centerPos + originalOffsets[i];
            pieces[i].rectTransform.localScale = Vector3.one;
        }
        
        foreach (PuzzlePiece piece in pieces) {
            piece.isAnimating = false;
        }
    }
    
    private float EaseOutBack(float t) {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
