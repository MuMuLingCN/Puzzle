using UnityEngine;
using UnityEditor;
using System.IO;

public class PuzzleImageHelper : EditorWindow
{
    private Texture2D sourceTexture;
    private int maxResolution = 2048;

    [MenuItem("Tools/拼图图片工具")]
    public static void ShowWindow()
    {
        GetWindow<PuzzleImageHelper>("拼图图片工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("拼图图片设置工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("此工具帮助你设置拼图图片的最佳参数", MessageType.Info);
        EditorGUILayout.Space();

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("选择图片", sourceTexture, typeof(Texture2D), false);

        if (sourceTexture != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前图片信息:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"尺寸: {sourceTexture.width} x {sourceTexture.height}");
            EditorGUILayout.LabelField($"格式: {sourceTexture.format}");
            
            if (sourceTexture.width > 2048 || sourceTexture.height > 2048)
            {
                EditorGUILayout.HelpBox($"图片分辨率过高！建议最大使用 {maxResolution}x{maxResolution}", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("图片分辨率合适", MessageType.Info);
            }

            EditorGUILayout.Space();

            maxResolution = EditorGUILayout.IntSlider("最大分辨率", maxResolution, 512, 4096);

            EditorGUILayout.Space();

            if (GUILayout.Button("优化图片设置", GUILayout.Height(40)))
            {
                OptimizeImageSettings();
            }

            if (GUILayout.Button("导出优化后的图片", GUILayout.Height(30)))
            {
                ExportOptimizedImage();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("提示: 图片文件名包含'puzzle'或'Puzzle'会自动应用最佳设置", MessageType.Info);
    }

    private void OptimizeImageSettings()
    {
        if (sourceTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一张图片！", "确定");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.compressionQuality = 100;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.filterMode = FilterMode.Bilinear;
            settings.mipmapEnabled = false;
            settings.readable = true;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
            EditorUtility.DisplayDialog("成功", "图片设置已优化！\n现在可以用于拼图游戏了。", "确定");
        }
    }

    private void ExportOptimizedImage()
    {
        if (sourceTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一张图片！", "确定");
            return;
        }

        string path = EditorUtility.SaveFilePanel("保存优化后的图片", 
            Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceTexture)),
            Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sourceTexture)) + "_optimized",
            "png");

        if (!string.IsNullOrEmpty(path))
        {
            int newWidth = sourceTexture.width;
            int newHeight = sourceTexture.height;

            if (newWidth > maxResolution || newHeight > maxResolution)
            {
                float aspectRatio = (float)newWidth / newHeight;
                if (newWidth > newHeight)
                {
                    newWidth = maxResolution;
                    newHeight = Mathf.RoundToInt(maxResolution / aspectRatio);
                }
                else
                {
                    newHeight = maxResolution;
                    newWidth = Mathf.RoundToInt(maxResolution * aspectRatio);
                }
            }

            Texture2D resizedTexture = ResizeTexture(sourceTexture, newWidth, newHeight);
            byte[] bytes = resizedTexture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            DestroyImmediate(resizedTexture);

            EditorUtility.DisplayDialog("成功", $"图片已导出到:\n{path}\n尺寸: {newWidth}x{newHeight}", "确定");
            
            AssetDatabase.Refresh();
        }
    }

    private Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        RenderTexture.active = rt;
        
        Graphics.Blit(source, rt);
        
        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
}
