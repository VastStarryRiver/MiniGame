using Invariable;
using System.IO;
using UnityEditor;



namespace MyTools
{
    public class ConfigTool
    {
        /// <summary>
        /// 导出 WebData 为 bin
        /// </summary>
        [MenuItem("VastStarryRiver/Config/导出Web配置", false, 0)]
        public static void BuildWebBinFile()
        {
            using (FileStream fileStream = new FileStream(ConfigUtils.m_localRootPath + "WebData.txt", FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    ConfigUtils.SaveSafeFile(streamReader.ReadToEnd(), ConfigUtils.LocalResourcePath + "/WebData.bin");
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 重新导出全部 Excel 配置
        /// </summary>
        [MenuItem("VastStarryRiver/Config/导出Excel配置", false, 1)]
        public static void RebuildAll()
        {
            ConfigImporter.RebuildAll();
        }

        /// <summary>
        /// 校验全部配置数据
        /// </summary>
        [MenuItem("VastStarryRiver/Config/校验配置数据", false, 2)]
        public static void ValidateAll()
        {
            ConfigValidator.ValidateAll();
        }
    }
}