using Invariable;
using UnityEditor;
using UnityEngine;



namespace MyTools
{
    public static class AudioProcess
    {
        private const string BgmFolder = "Assets/GameAssets/Audios/Bgm";
        private const string SfxFolder = "Assets/GameAssets/Audios/Sfx";



        /// <summary>
        /// 按 Bgm/Sfx 目录批量设置音频导入格式
        /// </summary>
        [MenuItem("VastStarryRiver/资源处理/设置音频资源", false, 40)]
        public static void ApplyAudioImportSettings()
        {
            int bgmCount = ApplyFolder(BgmFolder, true);
            int sfxCount = ApplyFolder(SfxFolder, false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            GameLog.Info($"音频导入格式已设置: BGM {bgmCount} 个，音效 {sfxCount} 个");
        }



        /// <summary>
        /// 对指定目录下音频应用 BGM 或音效导入设置
        /// </summary>
        private static int ApplyFolder(string folder, bool isBgm)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                GameLog.Error($"音频目录不存在: {folder}");

                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
            int count = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;

                if (importer == null)
                {
                    continue;
                }

                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = isBgm ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
                settings.preloadAudioData = true;
                importer.defaultSampleSettings = settings;
                importer.loadInBackground = true;
                importer.forceToMono = !isBgm; // 音效强制单声道，BGM 保留立体声
                Disable3D(importer);
                importer.SaveAndReimport();
                count++;
            }

            return count;
        }

        /// <summary>
        /// 关闭音频导入的 3D 标记，避免无意义的空间化运算
        /// </summary>
        private static void Disable3D(AudioImporter importer)
        {
            SerializedObject serializedObject = new SerializedObject(importer);
            SerializedProperty spatialProperty = serializedObject.FindProperty("3D");

            if (spatialProperty == null)
            {
                spatialProperty = serializedObject.FindProperty("m_3D");
            }

            if (spatialProperty == null || !spatialProperty.boolValue)
            {
                return;
            }

            spatialProperty.boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}