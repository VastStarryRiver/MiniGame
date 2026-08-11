using Invariable;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;



namespace MyTools
{
    public static class ConfigImporter
    {
        public const string BytesDir = "Assets/GameAssets/Config";
        public const string GeneratedDir = "Assets/Scripts/HotUpdate/Config/Generated";

        /// <summary>
        /// Excel 配置源目录
        /// </summary>
        public static string SourceDir
        {
            get
            {
                return ConfigUtils.ConfigExcelPath;
            }
        }



        /// <summary>
        /// 重新导出全部 Excel 为 bytes 与生成代码
        /// </summary>
        public static void RebuildAll()
        {
            try
            {
                string sourceDir = SourceDir;

                if (!Directory.Exists(sourceDir))
                {
                    Directory.CreateDirectory(sourceDir);
                    GameLog.Info($"Created {sourceDir}. Put .xlsx/.xls tables there.");

                    return;
                }

                Directory.CreateDirectory(BytesDir);
                Directory.CreateDirectory(GeneratedDir);
                ClearAllProducts();

                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.TopDirectoryOnly);
                var tableNames = new List<string>();
                int count = 0;

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();

                    if (ext != ".xlsx" && ext != ".xls")
                    {
                        continue;
                    }

                    string tableName = Path.GetFileNameWithoutExtension(file);

                    if (!FieldAnalyzer.IsValidIdentifier(tableName))
                    {
                        FailRebuild($"表名非法（须为合法 C# 标识符）: {file}");

                        return;
                    }

                    string abs = Path.GetFullPath(file);
                    EditorUtility.DisplayProgressBar("导出配置表", tableName, (float)count / Math.Max(1, files.Length));
                    string[][] rows = ExcelReader.ReadTable(abs);

                    if (rows == null)
                    {
                        FailRebuild($"读取失败: {file}");

                        return;
                    }

                    AnalyzedTable analyzed = FieldAnalyzer.Analyze(tableName, abs, rows);

                    if (analyzed == null)
                    {
                        FailRebuild($"表分析失败: {file}");

                        return;
                    }

                    if (!ConfigBinaryWriter.Export(analyzed, BytesDir))
                    {
                        FailRebuild($"bytes 导出失败: {file}");

                        return;
                    }

                    CodeGenerator.Generate(analyzed, GeneratedDir);
                    tableNames.Add(tableName);
                    count++;
                }

                tableNames.Sort(StringComparer.Ordinal);
                CodeGenerator.GeneratePreload(tableNames.ToArray(), GeneratedDir);

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                GameLog.Info($"Config rebuild done. Tables: {count}");
            }
            catch (Exception error)
            {
                FailRebuild($"导表异常中断: {error}");
            }
        }



        /// <summary>
        /// 导表失败：清空全部产物并提示
        /// </summary>
        private static void FailRebuild(string message)
        {
            EditorUtility.ClearProgressBar();
            ClearAllProducts();
            AssetDatabase.Refresh();
            GameLog.Error(message);
            GameLog.Error("导表已中断，已清空全部配置产物。请修复问题后重新导出，否则无法发布。");
        }

        /// <summary>
        /// 清空 bytes 与 Generated 全部产物
        /// </summary>
        private static void ClearAllProducts()
        {
            if (Directory.Exists(GeneratedDir))
            {
                foreach (string file in Directory.GetFiles(GeneratedDir, "Config_*.cs"))
                {
                    DeleteAssetFile(file);
                }

                string preloadPath = Path.Combine(GeneratedDir, "ConfigManager.Preload.cs");

                if (File.Exists(preloadPath))
                {
                    DeleteAssetFile(preloadPath);
                }
            }

            if (Directory.Exists(BytesDir))
            {
                foreach (string file in Directory.GetFiles(BytesDir, "*.bytes"))
                {
                    DeleteAssetFile(file);
                }
            }
        }

        /// <summary>
        /// 删除资源文件及其 .meta
        /// </summary>
        private static void DeleteAssetFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string meta = path + ".meta";

            if (File.Exists(meta))
            {
                File.Delete(meta);
            }
        }
    }
}
