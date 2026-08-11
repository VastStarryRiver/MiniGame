using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    /// <summary>
    /// 配置表运行时底座（首包，不随导表变化）
    /// HotUpdate 侧通过 ConfigManager 转发访问
    /// </summary>
    public static class ConfigManagerCore
    {
        private enum LoadState
        {
            None,
            Loading,
            Loaded
        }

        public interface IConfigData
        {
            string TableName
            {
                get;
            }

            bool IsLoaded
            {
                get;
            }

            float LastAccessTime
            {
                get;
            }

            void Evict();
        }

        public sealed class ConfigData<TConfig> : IConfigData where TConfig : ConfigBase, new()
        {
            private LoadState m_state = LoadState.None;
            private readonly List<Action<ConfigData<TConfig>>> Pending = new List<Action<ConfigData<TConfig>>>();
            private readonly int ExpectedSchemaHash;

            public string TableName
            {
                get;
            }

            public ConfigReader Reader
            {
                get;
                private set;
            }

            public DictionaryForConfig<TConfig> DicData
            {
                get;
                private set;
            }

            public bool IsLoaded
            {
                get
                {
                    return m_state == LoadState.Loaded;
                }
            }

            public float LastAccessTime
            {
                get;
                private set;
            }



            public ConfigData(string configName, int expectedSchemaHash)
            {
                TableName = configName;
                ExpectedSchemaHash = expectedSchemaHash;
            }



            /// <summary>
            /// 刷新最近访问时间
            /// </summary>
            public void Touch()
            {
                LastAccessTime = Time.realtimeSinceStartup;
            }

            /// <summary>
            /// 确保配置已加载，完成后回调
            /// </summary>
            public void EnsureLoaded(Action<ConfigData<TConfig>> callBack)
            {
                if (callBack == null)
                {
                    return;
                }

                Touch();

                if (m_state == LoadState.Loaded)
                {
                    InvokeIsolated(callBack, this);

                    return;
                }

                Pending.Add(callBack);

                if (m_state == LoadState.Loading)
                {
                    return;
                }

                m_state = LoadState.Loading;
                LoadBytes(TableName, (data) =>
                {
                    if (data == null)
                    {
                        FailPending($"Missing config data: Config_{TableName}");

                        return;
                    }

                    Reader = new ConfigReader(TableName, data, ExpectedSchemaHash);

                    if (!Reader.IsValid)
                    {
                        Reader = null;
                        FailPending($"Config schema mismatch or corrupt: Config_{TableName}");

                        return;
                    }

                    DicData = new DictionaryForConfig<TConfig>(TableName, Reader.Ids, ReadData, Touch);
                    m_state = LoadState.Loaded;
                    Touch();
                    FlushPending();
                });
            }

            /// <summary>
            /// 逐出已加载的配置解析层
            /// </summary>
            public void Evict()
            {
                if (m_state == LoadState.None)
                {
                    return;
                }

                if (DicData != null)
                {
                    DicData.Invalidate();
                }

                Reader = null;
                DicData = null;
                m_state = LoadState.None;

                if (Pending.Count <= 0)
                {
                    return;
                }

                Action<ConfigData<TConfig>>[] callbacks = Pending.ToArray();
                Pending.Clear();

                for (int i = 0; i < callbacks.Length; i++)
                {
                    InvokeIsolated(callbacks[i], null);
                }
            }

            /// <summary>
            /// 按行下标反序列化一条配置
            /// </summary>
            private ConfigBase ReadData(int index)
            {
                if (Reader == null)
                {
                    GameLog.Error($"Config reader disposed: {TableName}");

                    return null;
                }

                TConfig row = new TConfig();
                Reader.BeginRow(index);
                row.Deserialize(Reader);

                return row;
            }

            /// <summary>
            /// 加载失败时清理状态并回调等待者
            /// </summary>
            private void FailPending(string message)
            {
                GameLog.Error(message);
                m_state = LoadState.None;
                Reader = null;
                DicData = null;
                Action<ConfigData<TConfig>>[] callbacks = Pending.ToArray();
                Pending.Clear();

                for (int i = 0; i < callbacks.Length; i++)
                {
                    InvokeIsolated(callbacks[i], null);
                }
            }

            /// <summary>
            /// 刷新并执行全部等待中的加载回调
            /// </summary>
            private void FlushPending()
            {
                Action<ConfigData<TConfig>>[] callbacks = Pending.ToArray();
                Pending.Clear();

                for (int i = 0; i < callbacks.Length; i++)
                {
                    InvokeIsolated(callbacks[i], this);
                }
            }

            /// <summary>
            /// 隔离执行配置回调，避免异常中断流程
            /// </summary>
            private void InvokeIsolated(Action<ConfigData<TConfig>> callBack, ConfigData<TConfig> arg)
            {
                if (callBack == null)
                {
                    return;
                }

                try
                {
                    callBack(arg);
                }
                catch (Exception error)
                {
                    GameLog.Error($"Config callback error [{TableName}]: {error}");
                }
            }
        }



        private static readonly List<IConfigData> ActiveTables = new List<IConfigData>();

        /// <summary>
        /// YooAsset 寻址约定：Config_{configName}
        /// （与 AddressByGroupAndFileName 一致，同 Prefabs_MainPanel）
        /// </summary>
        public static void LoadBytes(string nameWithoutExtension, Action<byte[]> callBack)
        {
            if (callBack == null)
            {
                return;
            }

            string address = $"Config_{nameWithoutExtension}";
            YooAssetManager.Instance.AsyncLoadAsset<TextAsset>(address, (asset) =>
            {
                if (asset == null)
                {
                    GameLog.Error($"Load config TextAsset failed: {address}");
                    callBack(null);

                    return;
                }

                callBack(asset.bytes);
            });
        }

        /// <summary>
        /// 加载指定配置表并在完成后回调
        /// </summary>
        public static void LoadConfigData<TConfig>(ref ConfigData<TConfig> configData, string configName, int expectedSchemaHash, Action<ConfigData<TConfig>> callBack) where TConfig : ConfigBase, new()
        {
            if (configData == null)
            {
                configData = new ConfigData<TConfig>(configName, expectedSchemaHash);
                ActiveTables.Add(configData);
            }

            configData.EnsureLoaded(callBack);
        }

        /// <summary>
        /// 清除并逐出指定配置表
        /// </summary>
        public static void ClearConfigData<TConfig>(ref ConfigData<TConfig> configData) where TConfig : ConfigBase, new()
        {
            if (configData != null)
            {
                configData.Evict();
                ActiveTables.Remove(configData);
            }

            configData = null;
        }

        /// <summary>
        /// 逐出闲置超过阈值且已 Loaded 的表（仅清解析层，保留 YooAsset handle）
        /// </summary>
        public static void TickEvict()
        {
            float now = Time.realtimeSinceStartup;

            for (int i = 0; i < ActiveTables.Count; i++)
            {
                IConfigData table = ActiveTables[i];

                if (table.IsLoaded && (now - table.LastAccessTime) >= ConfigFormat.EvictIdleSeconds)
                {
                    table.Evict();
                }
            }
        }

        /// <summary>
        /// 加载并物化整表。行数超过阈值时分帧，完成才回调完整列表。
        /// </summary>
        public static void LoadAllSliced<TConfig>(
            ref ConfigData<TConfig> configData,
            string configName,
            int expectedSchemaHash,
            Action<IReadOnlyList<TConfig>> onComplete,
            Action<int, int> onProgress) where TConfig : ConfigBase, new()
        {
            if (onComplete == null)
            {
                return;
            }

            LoadConfigData(ref configData, configName, expectedSchemaHash, (data) =>
            {
                if (data == null || data.DicData == null)
                {
                    InvokeActionIsolated(configName, () => onComplete(null));

                    return;
                }

                DictionaryForConfig<TConfig> dic = data.DicData;
                int total = dic.Count;

                if (total <= ConfigFormat.GetAllSliceThreshold || !GameManager.HasInstance)
                {
                    IReadOnlyList<TConfig> all = dic.GetAll();
                    data.Touch();
                    InvokeActionIsolated(configName, () => onProgress?.Invoke(total, total));
                    InvokeActionIsolated(configName, () => onComplete(all));

                    return;
                }

                GameManager.Instance.StartCoroutine(MaterializeCoroutine(data, dic, onComplete, onProgress));
            });
        }

        /// <summary>
        /// 分帧物化整表协程
        /// </summary>
        private static IEnumerator MaterializeCoroutine<TConfig>(
            ConfigData<TConfig> data,
            DictionaryForConfig<TConfig> dic,
            Action<IReadOnlyList<TConfig>> onComplete,
            Action<int, int> onProgress) where TConfig : ConfigBase, new()
        {
            int index = 0;
            int total = dic.Count;
            string tableName = data.TableName;

            while (index < total)
            {
                if (!data.IsLoaded || data.DicData != dic || !dic.IsAlive)
                {
                    InvokeActionIsolated(tableName, () => onComplete(null));

                    yield break;
                }

                data.Touch();
                index = dic.MaterializeRange(index, ConfigFormat.GetAllRowsPerFrame);
                int progressIndex = index;
                InvokeActionIsolated(tableName, () => onProgress?.Invoke(progressIndex, total));

                if (index < total)
                {
                    yield return null;
                }
            }

            IReadOnlyList<TConfig> all = dic.GetAll();
            data.Touch();
            InvokeActionIsolated(tableName, () => onComplete(all));
        }

        /// <summary>
        /// 隔离执行无参回调，避免异常中断流程
        /// </summary>
        private static void InvokeActionIsolated(string tableName, Action action)
        {
            if (action == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (Exception error)
            {
                GameLog.Error($"Config callback error [{tableName}]: {error}");
            }
        }
    }
}