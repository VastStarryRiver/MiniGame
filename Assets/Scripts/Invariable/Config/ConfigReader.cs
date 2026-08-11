using System;
using System.Collections.Generic;
using UnityEngine;



namespace Invariable
{
    public sealed class ConfigReader
    {
        private readonly string ConfigNameValue;
        private readonly int RowCountValue;
        private readonly int RowSizeValue;
        private readonly int[] IdsValue;
        private readonly byte[] Data;
        private readonly int DataStart;
        private readonly int StringRegionStart;
        private readonly Dictionary<int, string> StringCache = new Dictionary<int, string>();
        private BinReader m_reader;

        public string TableName
        {
            get
            {
                return ConfigNameValue;
            }
        }

        public int RowCount
        {
            get
            {
                return RowCountValue;
            }
        }

        public int RowSize
        {
            get
            {
                return RowSizeValue;
            }
        }

        public int[] Ids
        {
            get
            {
                return IdsValue;
            }
        }

        public bool IsValid
        {
            get;
        }



        /// <summary>
        /// 合并格式：magic + schemaHash + count + ids + rowSize + 数据区 + 字符串区
        /// </summary>
        public ConfigReader(string configName, byte[] dataBytes, int expectedSchemaHash)
        {
            ConfigNameValue = configName;
            Data = dataBytes ?? Array.Empty<byte>();
            m_reader = new BinReader(Data, 0);
            IsValid = false;
            RowCountValue = 0;
            IdsValue = Array.Empty<int>();
            RowSizeValue = 0;
            DataStart = 0;
            StringRegionStart = 0;

            if (Data.Length < 16)
            {
                GameLog.Error($"Config bytes too short: {configName}");

                return;
            }

            int magic = m_reader.ReadInt32();

            if (magic != ConfigFormat.Magic)
            {
                GameLog.Error($"Config magic mismatch: {configName}, expected CFGT, got 0x{magic:X8}. Please re-export Excel config.");

                return;
            }

            int schemaHash = m_reader.ReadInt32();

            if (schemaHash != expectedSchemaHash)
            {
                GameLog.Error($"Config schema mismatch: {configName}, code=0x{expectedSchemaHash:X8}, bytes=0x{schemaHash:X8}. Table and generated code are out of sync.");

                return;
            }

            int rowCount = m_reader.ReadInt32();

            if (rowCount < 0)
            {
                GameLog.Error($"Invalid RowCount for table {configName}");
                rowCount = 0;
            }

            RowCountValue = rowCount;
            IdsValue = new int[RowCountValue];

            for (int i = 0; i < RowCountValue; i++)
            {
                IdsValue[i] = m_reader.ReadInt32();
            }

            int rowSize = m_reader.ReadInt32();

            if (RowCountValue > 0 && rowSize <= 0)
            {
                GameLog.Error($"Invalid RowSize for table {configName}");
                rowSize = 0;
            }

            RowSizeValue = rowSize;
            // magic(4) + schemaHash(4) + count(4) + ids(4N) + rowSize(4)
            DataStart = 16 + 4 * RowCountValue;
            StringRegionStart = DataStart + RowSizeValue * RowCountValue;
            IsValid = true;
        }



        /// <summary>
        /// 定位到指定行起始位置
        /// </summary>
        public void BeginRow(int index)
        {
            if (index < 0 || index >= RowCountValue)
            {
                GameLog.Error($"BeginRow index out of range: {index}");

                return;
            }

            m_reader.Seek(DataStart + index * RowSizeValue);
        }

        /// <summary>
        /// 一次性读取行内全部 4 字节槽位，减少 HotUpdate→AOT 跨界调用次数
        /// </summary>
        public void ReadSlots(int[] slots, int count)
        {
            if (slots == null || count <= 0)
            {
                return;
            }

            if (count > slots.Length)
            {
                GameLog.Error($"ReadSlots count exceeds buffer: {count} > {slots.Length}");

                return;
            }

            int byteCount = count * 4;

            if (m_reader.m_position + byteCount > Data.Length)
            {
                GameLog.Error($"ReadSlots out of range: table={ConfigNameValue}");

                return;
            }

            Buffer.BlockCopy(Data, m_reader.m_position, slots, 0, byteCount);
            m_reader.m_position += byteCount;
        }

        /// <summary>
        /// 按字符串区偏移读取并缓存字符串
        /// </summary>
        public string ReadStringByOffset(int offset)
        {
            if (StringCache.TryGetValue(offset, out string cached))
            {
                return cached;
            }

            int saved = m_reader.m_position;
            m_reader.Seek(StringRegionStart + offset);
            string value = m_reader.ReadStringRecord();
            StringCache[offset] = value;
            m_reader.Seek(saved);

            return value;
        }

        /// <summary>
        /// 读取一个 Int32 字段
        /// </summary>
        public void Read(out int value)
        {
            value = m_reader.ReadInt32();
        }

        /// <summary>
        /// 读取一个 Single 字段
        /// </summary>
        public void Read(out float value)
        {
            value = m_reader.ReadSingle();
        }

        /// <summary>
        /// 读取一个字符串字段（按偏移解析）
        /// </summary>
        public void Read(out string value)
        {
            int offset = m_reader.ReadInt32();
            value = ReadStringByOffset(offset);
        }

        /// <summary>
        /// 读取固定长度的 Int32 数组
        /// </summary>
        public void Read(out int[] value, int length)
        {
            value = new int[length];

            for (int i = 0; i < length; i++)
            {
                value[i] = m_reader.ReadInt32();
            }
        }

        /// <summary>
        /// 读取固定长度的 Single 数组
        /// </summary>
        public void Read(out float[] value, int length)
        {
            value = new float[length];

            for (int i = 0; i < length; i++)
            {
                value[i] = m_reader.ReadSingle();
            }
        }

        /// <summary>
        /// 读取固定长度的字符串数组
        /// </summary>
        public void Read(out string[] value, int length)
        {
            value = new string[length];

            for (int i = 0; i < length; i++)
            {
                Read(out value[i]);
            }
        }
    }
}