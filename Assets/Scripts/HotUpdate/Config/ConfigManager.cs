using Invariable;
using System;
using System.Collections.Generic;



namespace HotUpdate
{
    /// <summary>
    /// 配置访问入口，转发到 Invariable.ConfigManagerCore
    /// Generated/Config_*.cs 中的 partial 与本类合并
    /// </summary>
    public static partial class ConfigManager
    {
        /// <summary>
        /// 加载指定配置表数据
        /// </summary>
        public static void LoadConfigData<TConfig>(ref ConfigManagerCore.ConfigData<TConfig> configData, string configName, int expectedSchemaHash, Action<ConfigManagerCore.ConfigData<TConfig>> callBack) where TConfig : ConfigBase, new()
        {
            ConfigManagerCore.LoadConfigData(ref configData, configName, expectedSchemaHash, callBack);
        }

        /// <summary>
        /// 清理指定配置表数据
        /// </summary>
        public static void ClearConfigData<TConfig>(ref ConfigManagerCore.ConfigData<TConfig> configData) where TConfig : ConfigBase, new()
        {
            ConfigManagerCore.ClearConfigData(ref configData);
        }

        /// <summary>
        /// 分片加载配置表全部数据
        /// </summary>
        public static void LoadAllSliced<TConfig>(
            ref ConfigManagerCore.ConfigData<TConfig> configData,
            string configName,
            int expectedSchemaHash,
            Action<IReadOnlyList<TConfig>> onComplete,
            Action<int, int> onProgress) where TConfig : ConfigBase, new()
        {
            ConfigManagerCore.LoadAllSliced(ref configData, configName, expectedSchemaHash, onComplete, onProgress);
        }
    }
}