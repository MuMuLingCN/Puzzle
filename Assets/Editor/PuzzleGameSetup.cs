using UnityEngine;
using UnityEditor;

public class PuzzleGameSetup : EditorWindow
{
    private Sprite puzzleSprite;
    private int gridSize = 5;
    private float puzzleSize = 600f;
    private float pieceSpacing = 2f;

    [MenuItem("Tools/拼图游戏设置")]
    public static void ShowWindow()
    {
        GetWindow<PuzzleGameSetup>("拼图游戏设置");
    }

    private void OnGUI()
    {
        GUILayout.Label("拼图游戏设置", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        puzzleSprite = (Sprite)EditorGUILayout.ObjectField("拼图图片", puzzleSprite, typeof(Sprite), false);
        
        if (puzzleSprite != null)
        {
            EditorGUILayout.HelpBox($"图片尺寸: {puzzleSprite.rect.width} x {puzzleSprite.rect.height}\n宽高比: {(puzzleSprite.rect.width / puzzleSprite.rect.height):F2}", MessageType.Info);
        }
        
        gridSize = EditorGUILayout.IntSlider("网格大小", gridSize, 2, 10);
        puzzleSize = EditorGUILayout.Slider("拼图大小", puzzleSize, 300f, 800f);
        pieceSpacing = EditorGUILayout.Slider("拼图块间距", pieceSpacing, 0f, 10f);

        EditorGUILayout.Space();

        if (GUILayout.Button("创建拼图游戏场景", GUILayout.Height(40)))
        {
            CreatePuzzleGameScene();
        }

        if (GUILayout.Button("清空场景", GUILayout.Height(30)))
        {
            ClearScene();
        }
    }

    private void CreatePuzzleGameScene()
    {
        if (puzzleSprite == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择一张拼图图片！", "确定");
            return;
        }

        CreateSceneInitializer();

        EditorUtility.DisplayDialog("成功", "拼图游戏场景创建完成！\n请运行游戏查看效果。", "确定");
    }

    private void CreateSceneInitializer()
    {
        GameObject initializerObj = new GameObject("SceneInitializer");
        SceneInitializer initializer = initializerObj.AddComponent<SceneInitializer>();
        
        initializer.puzzleSprite = puzzleSprite;
        initializer.gridSize = gridSize;
        initializer.puzzleSize = puzzleSize;
        initializer.pieceSpacing = pieceSpacing;

        EditorUtility.SetDirty(initializerObj);
    }

    private void ClearScene()
    {
        if (EditorUtility.DisplayDialog("确认清空", "确定要清空场景中的所有对象吗？", "确定", "取消"))
        {
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj != null && obj.transform.parent == null)
                {
                    DestroyImmediate(obj);
                }
            }
        }
    }
}
