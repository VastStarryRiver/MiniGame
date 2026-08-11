using Invariable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;



namespace MyTools
{
    public sealed class ConfigBinaryWriter : IDisposable
    {
        private readonly MemoryStream MemoryStream = new MemoryStream();
        private readonly BinaryWriter BinaryWriter;
        private readonly Dictionary<string, int> PosList = new Dictionary<string, int>();
        private readonly List<string> StrList = new List<string>();
        private int m_nextStrPos;

        /// <summary>
        /// 当前已写入字节长度
        /// </summary>
        public int Length
        {
            get
            {
                return (int)MemoryStream.Length;
            }
        }



        public ConfigBinaryWriter()
        {
            BinaryWriter = new BinaryWriter(MemoryStream, Encoding.Unicode, leaveOpen: true);
        }



        /// <summary>
        /// 写入 Int32
        /// </summary>
        public void WriteInt32(int value)
        {
            BinaryWriter.Write(value);
        }

        /// <summary>
        /// 写入 Single
        /// </summary>
        public void WriteSingle(float value)
        {
            BinaryWriter.Write(value);
        }

        /// <summary>
        /// 写入原始字节
        /// </summary>
        public void WriteRawByte(byte value)
        {
            BinaryWriter.Write(value);
        }

        /// <summary>
        /// 导出当前缓冲区字节数组
        /// </summary>
        public byte[] ToArray()
        {
            return MemoryStream.ToArray();
        }

        /// <summary>
        /// 释放底层流资源
        /// </summary>
        public void Dispose()
        {
            BinaryWriter.Dispose();
            MemoryStream.Dispose();
        }

        /// <summary>
        /// 写入字符串偏移（去重后计入字符串区）
        /// </summary>
        public void WriteStringOffset(string value)
        {
            value = value ?? "";

            if (!PosList.TryGetValue(value, out int pos))
            {
                pos = m_nextStrPos;

                PosList.Add(value, pos);
                StrList.Add(value);

                int size = 4 + value.Length * 2;
                int pad = (4 - (size & 3)) & 3;

                m_nextStrPos += size + pad;
            }

            WriteInt32(pos);
        }

        /// <summary>
        /// 写出全部去重字符串记录
        /// </summary>
        public void WriteAllStrings()
        {
            foreach (string str in StrList)
            {
                WriteInt32(str.Length);

                if (str.Length > 0)
                {
                    BinaryWriter.Write(Encoding.Unicode.GetBytes(str));
                }

                int size = 4 + str.Length * 2;
                int pad = (4 - (size & 3)) & 3;

                for (int i = 0; i < pad; i++)
                {
                    WriteRawByte(0);
                }
            }
        }

        /// <summary>
        /// 覆盖指定绝对偏移处的 Int32
        /// </summary>
        public void OverwriteInt32(int absoluteOffset, int value)
        {
            long cur = MemoryStream.Position;
            MemoryStream.Position = absoluteOffset;
            BinaryWriter.Write(value);
            MemoryStream.Position = cur;
        }

        /// <summary>
        /// 将分析后的表导出为 .bytes
        /// </summary>
        public static bool Export(AnalyzedTable table, string bytesDir)
        {
            Directory.CreateDirectory(bytesDir);

            var ids = new List<int>();

            for (int row = table.DataStartRow; row < table.Rows.Length; row++)
            {
                string idText = FieldAnalyzer.Cell(table.Rows, row, 0);

                if (!int.TryParse(idText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    GameLog.Error($"[{table.TableName}] 第 {row + 1} 行, 字段 \"Id\"(int): 值 \"{idText}\" 解析失败。Path:{table.SourcePath}");

                    return false;
                }

                ids.Add(id);
            }

            using (var configBinaryWriter = new ConfigBinaryWriter())
            {
                configBinaryWriter.WriteInt32(Invariable.ConfigFormat.Magic);
                configBinaryWriter.WriteInt32(ConfigSchemaHash.Compute(table));
                configBinaryWriter.WriteInt32(ids.Count);

                for (int i = 0; i < ids.Count; i++)
                {
                    configBinaryWriter.WriteInt32(ids[i]);
                }

                int rowSize = -1;
                int rowSizeOffset = configBinaryWriter.Length;

                configBinaryWriter.WriteInt32(0);

                for (int row = table.DataStartRow; row < table.Rows.Length; row++)
                {
                    int start = configBinaryWriter.Length;

                    foreach (ConfigField field in table.Fields)
                    {
                        if (!WriteField(configBinaryWriter, table, field, row))
                        {
                            return false;
                        }
                    }

                    int rowLen = configBinaryWriter.Length - start;
                    int pad = (4 - (rowLen & 3)) & 3;

                    for (int i = 0; i < pad; i++)
                    {
                        configBinaryWriter.WriteRawByte(0);
                    }

                    int currSize = configBinaryWriter.Length - start;

                    if (rowSize < 0)
                    {
                        rowSize = currSize;
                    }
                    else if (currSize != rowSize)
                    {
                        GameLog.Error($"[{table.TableName}] 第 {row + 1} 行: 行大小不一致。Path:{table.SourcePath}");

                        return false;
                    }
                }

                if (rowSize < 0)
                {
                    rowSize = 0;
                }

                configBinaryWriter.OverwriteInt32(rowSizeOffset, rowSize);
                configBinaryWriter.WriteAllStrings();

                File.WriteAllBytes(Path.Combine(bytesDir, table.TableName + ".bytes"), configBinaryWriter.ToArray());
            }

            return true;
        }



        /// <summary>
        /// 写入单个字段（标量或数组）
        /// </summary>
        private static bool WriteField(ConfigBinaryWriter configBinaryWriter, AnalyzedTable analyzedTable, ConfigField configField, int row)
        {
            if (configField.ArrayLength > 0)
            {
                for (int i = 0; i < configField.ArrayLength; i++)
                {
                    string content = FieldAnalyzer.Cell(analyzedTable.Rows, row, configField.SourceColumns[i]);

                    if (!WriteScalar(configBinaryWriter, analyzedTable, configField, row, content))
                    {
                        return false;
                    }
                }
            }
            else
            {
                string content = FieldAnalyzer.Cell(analyzedTable.Rows, row, configField.SourceColumns[0]);

                if (!WriteScalar(configBinaryWriter, analyzedTable, configField, row, content))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 按原始类型写入标量值
        /// </summary>
        private static bool WriteScalar(ConfigBinaryWriter configBinaryWriter, AnalyzedTable table, ConfigField field, int row, string content)
        {
            content = content ?? "";

            try
            {
                switch (field.Primitive)
                {
                    case ConfigPrimitive.Int:
                        configBinaryWriter.WriteInt32(string.IsNullOrWhiteSpace(content) ? 0 : int.Parse(content, CultureInfo.InvariantCulture));
                        break;

                    case ConfigPrimitive.Float:
                        configBinaryWriter.WriteSingle(string.IsNullOrWhiteSpace(content) ? 0f : float.Parse(content, CultureInfo.InvariantCulture));
                        break;

                    case ConfigPrimitive.String:
                        configBinaryWriter.WriteStringOffset(content);
                        break;
                }
            }
            catch (Exception)
            {
                GameLog.Error($"[{table.TableName}] 第 {row + 1} 行, 字段 \"{field.Name}\"({field.CsType}): 值 \"{content}\" 解析失败。Path:{table.SourcePath}");

                return false;
            }

            return true;
        }
    }
}