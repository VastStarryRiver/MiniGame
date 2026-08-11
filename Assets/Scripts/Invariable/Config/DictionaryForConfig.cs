using System;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public sealed class DictionaryForConfig<TValue> where TValue : ConfigBase
    {
        private readonly string TableName;
        private readonly Dictionary<int, int> KeyToIndex;
        private readonly ConfigBase[] Values;
        private readonly Func<int, ConfigBase> LoadRow;
        private readonly Action OnAccess;
        private TValue[] m_allArray;
        private bool m_alive = true;

        public int Count
        {
            get;
        }

        public bool IsAlive
        {
            get
            {
                return m_alive;
            }
        }

        public Dictionary<int, int>.KeyCollection Keys
        {
            get
            {
                return KeyToIndex.Keys;
            }
        }



        public DictionaryForConfig(string tableName, int[] keys, Func<int, ConfigBase> loadRow, Action onAccess)
        {
            TableName = tableName;
            Count = keys.Length;
            KeyToIndex = new Dictionary<int, int>(Count);

            for (int i = 0; i < Count; i++)
            {
                KeyToIndex.Add(keys[i], i);
            }

            Values = new ConfigBase[Count];
            LoadRow = loadRow;
            OnAccess = onAccess;
        }



        /// <summary>
        /// 标记字典已失效（逐出后不可再访问）
        /// </summary>
        public void Invalidate()
        {
            m_alive = false;
        }

        public TValue this[int key]
        {
            get
            {
                if (!EnsureAlive())
                {
                    return null;
                }

                if (KeyToIndex.TryGetValue(key, out int index))
                {
                    return GetData(index);
                }

                GameLog.Info($"Config id not found: {TableName} id={key}");

                return null;
            }
        }

        /// <summary>
        /// 按行下标获取配置（懒加载）
        /// </summary>
        public TValue GetData(int index)
        {
            if (!EnsureAlive())
            {
                return null;
            }

            ConfigBase value = Values[index];

            if (value == null)
            {
                value = LoadRow(index);
                Values[index] = value;
            }

            return (TValue)value;
        }

        /// <summary>
        /// 判断是否包含指定配置 Id
        /// </summary>
        public bool ContainsKey(int key)
        {
            if (!EnsureAlive())
            {
                return false;
            }

            return KeyToIndex.ContainsKey(key);
        }

        /// <summary>
        /// 尝试按 Id 获取配置行
        /// </summary>
        public bool TryGetValue(int key, out TValue value)
        {
            value = null;

            if (!EnsureAlive())
            {
                return false;
            }

            if (!KeyToIndex.TryGetValue(key, out int index))
            {
                return false;
            }

            value = GetData(index);

            return true;
        }

        /// <summary>
        /// 同步物化全部行，返回内部缓存数组（重复调用零分配）
        /// </summary>
        public IReadOnlyList<TValue> GetAll()
        {
            MaterializeRange(0, Count);

            return m_allArray ?? Array.Empty<TValue>();
        }

        /// <summary>
        /// 物化 [start, start+count) 区间行，返回新的结束下标
        /// </summary>
        public int MaterializeRange(int start, int count)
        {
            if (!EnsureAlive())
            {
                return Count;
            }

            if (m_allArray == null)
            {
                m_allArray = Count > 0 ? new TValue[Count] : Array.Empty<TValue>();
            }

            if (Count == 0 || count <= 0 || start >= Count)
            {
                return Count;
            }

            if (start < 0)
            {
                start = 0;
            }

            int end = start + count;

            if (end > Count)
            {
                end = Count;
            }

            for (int i = start; i < end; i++)
            {
                m_allArray[i] = GetData(i);
            }

            return end;
        }

        /// <summary>
        /// 确认字典仍有效并刷新访问时间
        /// </summary>
        private bool EnsureAlive()
        {
            if (!m_alive)
            {
                GameLog.Error($"Config dictionary disposed: {TableName}. Do not cache DicData across eviction; call ConfigManager.GetXxx again.");

                return false;
            }

            OnAccess?.Invoke();

            return true;
        }
    }
}