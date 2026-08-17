using UnityEditor;



namespace MyTools
{
    public class ConfigTool
    {
        /// <summary>
        /// 重新导出全部 Excel 配置
        /// </summary>
        [MenuItem("VastStarryRiver/Config/导出Excel配置", false, 0)]
        public static void RebuildAll()
        {
            ConfigImporter.RebuildAll();
        }

        /// <summary>
        /// 校验全部配置数据
        /// </summary>
        [MenuItem("VastStarryRiver/Config/校验配置数据", false, 1)]
        public static void ValidateAll()
        {
            ConfigValidator.ValidateAll();
        }
    }
}