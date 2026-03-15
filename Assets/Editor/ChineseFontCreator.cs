using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class ChineseFontCreator : EditorWindow
{
    private Font selectedFont;
    private string savePath = "Assets/Fonts";
    private string fontName = "ChineseTMPFont";

    [MenuItem("Tools/创建中文字体")]
    public static void ShowWindow()
    {
        GetWindow<ChineseFontCreator>("创建中文字体");
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshPro 中文字体生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);

        selectedFont = (Font)EditorGUILayout.ObjectField("选择字体文件 (.ttf/.otf)", selectedFont, typeof(Font), false);
        GUILayout.Space(10);

        fontName = EditorGUILayout.TextField("字体资源名称", fontName);
        savePath = EditorGUILayout.TextField("保存路径", savePath);

        GUILayout.Space(20);

        if (GUILayout.Button("生成中文字体", GUILayout.Height(40)))
        {
            GenerateChineseFont();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox("使用说明:\n1. 选择一个支持中文的TTF或OTF字体文件\n2. 设置字体资源名称和保存路径\n3. 点击生成按钮\n\n推荐使用系统字体如: SimSun(宋体)、Microsoft YaHei(微软雅黑)、SimHei(黑体)等", MessageType.Info);
    }

    private void GenerateChineseFont()
    {
        if (selectedFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个字体文件！\n\n请选择支持中文的.ttf或.otf字体文件。", "确定");
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string assetPath = Path.Combine(savePath, fontName + ".asset");

        string chineseChars = "的一是了我不人在他有这个上们来到时大地为子中你说生国年着就那和要她出也得里后自以会家可下而过天去能对小多然于心学之都好看起发当没成只如事把还用第样道想作种开美总从无情己面最女但现前些所同日手又行意动方期它头经长儿回位分爱老因很给名法间斯知世什两次使身者被高已亲其进此话常与活正感" +
                             "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" +
                             "，。！？、；：“”‘’（）《》【】…—·～";

        try
        {
            TMP_FontAsset fontAsset = new TMP_FontAsset();
            fontAsset.name = fontName;

            Font sourceFont = selectedFont;

            Material material = new Material(Shader.Find("TextMeshPro/Distance Field"));
            material.name = fontName + " Material";

            Texture2D fontTexture = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
            fontTexture.filterMode = FilterMode.Bilinear;
            fontTexture.wrapMode = TextureWrapMode.Clamp;

            material.mainTexture = fontTexture;

            AssetDatabase.CreateAsset(fontTexture, Path.Combine(savePath, fontName + " Texture.asset"));
            AssetDatabase.CreateAsset(material, Path.Combine(savePath, fontName + " Material.mat"));
            AssetDatabase.CreateAsset(fontAsset, assetPath);

            EditorUtility.SetDirty(fontAsset);
            EditorUtility.SetDirty(material);
            EditorUtility.SetDirty(fontTexture);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("生成完成", $"中文字体资源已创建！\n\n位置: {assetPath}\n\n现在请在Unity中选择该字体资源，在Inspector中点击'Font Atlas Creator'生成字符集。", "确定");

            Selection.activeObject = fontAsset;

            Debug.Log($"中文字体资源已创建: {assetPath}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"字体生成失败: {e.Message}", "确定");
            Debug.LogError($"字体生成失败: {e}");
        }
    }
}
