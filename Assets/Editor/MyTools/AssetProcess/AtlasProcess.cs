using Invariable;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;



namespace MyTools
{
    public static class AtlasProcess
    {
        private const string AtlasFolder = "Assets/GameAssets/Atlas";
        private const string PngFolder = "Assets/GameAssets/Png";



        /// <summary>
        /// 批量设置图集、图集源图与散图的导入格式
        /// </summary>
        [MenuItem("VastStarryRiver/资源处理/设置图片和图集", false, 41)]
        public static void ApplyAtlasImportSettings()
        {
            int atlasCount = ApplyAtlases();
            int textureCount = ApplyTextures();
            int pngCount = ApplyPngTextures();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GameLog.Info($"图片和图集导入格式已设置: 图集 {atlasCount} 个，图集源图 {textureCount} 个，散图 {pngCount} 个");
        }



        /// <summary>
        /// 对 Atlas 目录下图集设置压缩并关闭可读
        /// </summary>
        private static int ApplyAtlases()
        {
            if (!AssetDatabase.IsValidFolder(AtlasFolder))
            {
                GameLog.Error($"图集目录不存在: {AtlasFolder}");

                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:SpriteAtlas", new[] { AtlasFolder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                SpriteAtlasImporter importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;

                if (importer == null)
                {
                    continue;
                }

                ApplyAtlasImporter(importer);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 对 Atlas 下 Texture 子目录图片应用最佳导入模式
        /// </summary>
        private static int ApplyTextures()
        {
            if (!AssetDatabase.IsValidFolder(AtlasFolder))
            {
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { AtlasFolder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!path.Contains("/Texture/") || !path.EndsWith(".png"))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                ApplySpriteTextureSettings(importer);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 对 Png 目录散图应用最佳导入模式
        /// </summary>
        private static int ApplyPngTextures()
        {
            if (!AssetDatabase.IsValidFolder(PngFolder))
            {
                GameLog.Error($"散图目录不存在: {PngFolder}");

                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PngFolder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (!path.EndsWith(".png"))
                {
                    continue;
                }

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer == null)
                {
                    continue;
                }

                ApplySpriteTextureSettings(importer);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 写入 UI 图片最佳导入模式：Sprite 类型、关可读、关 mipmap、压缩
        /// </summary>
        private static void ApplySpriteTextureSettings(TextureImporter importer)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
            }

            importer.isReadable = false;
            importer.mipmapEnabled = false;

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
            platformSettings.name = "DefaultTexturePlatform";
            platformSettings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(platformSettings);
        }

        /// <summary>
        /// 写入图集压缩与关闭可读
        /// </summary>
        private static void ApplyAtlasImporter(SpriteAtlasImporter importer)
        {
            SpriteAtlasTextureSettings textureSettings = importer.textureSettings;
            textureSettings.readable = false;
            importer.textureSettings = textureSettings;

            TextureImporterPlatformSettings platformSettings = importer.GetPlatformSettings("DefaultTexturePlatform");
            platformSettings.name = "DefaultTexturePlatform";
            platformSettings.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformSettings(platformSettings);

            SerializedObject serializedObject = new SerializedObject(importer);
            SerializedProperty textureSettingsProperty = serializedObject.FindProperty("m_TextureSettings");

            if (textureSettingsProperty == null)
            {
                textureSettingsProperty = serializedObject.FindProperty("textureSettings");
            }

            if (textureSettingsProperty == null)
            {
                return;
            }

            SerializedProperty compression = textureSettingsProperty.FindPropertyRelative("textureCompression");

            if (compression != null)
            {
                compression.intValue = (int)TextureImporterCompression.Compressed;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}