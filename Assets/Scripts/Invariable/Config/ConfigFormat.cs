namespace Invariable
{
    /// <summary>
    /// 配置表 bytes 文件格式常量
    /// 布局：magic + schemaHash + count + ids + rowSize + 数据区 + 字符串区
    /// </summary>
    public static class ConfigFormat
    {
        /// <summary>ASCII "CFGT" 小端 int32</summary>
        public const int Magic = 0x54474643;

        public const float EvictIdleSeconds = 180f;
        public const float EvictScanIntervalSeconds = 30f;
        public const int GetAllSliceThreshold = 500;
        public const int GetAllRowsPerFrame = 200;
    }
}