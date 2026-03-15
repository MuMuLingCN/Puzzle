using UnityEngine;
using UnityEditor;

public class PuzzleImagePostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath.Contains("Puzzle") || assetPath.Contains("puzzle"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            
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
        }
    }
}
