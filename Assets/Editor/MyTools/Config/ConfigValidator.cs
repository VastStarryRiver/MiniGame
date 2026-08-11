using Invariable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;



namespace MyTools
{
    /// <summary>
    /// Editor 独立校验器：回读 bytes 与 Excel 源逐字段比对，不引用 HotUpdate 程序集
    /// </summary>
    public static class ConfigValidator
    {
        /// <summary>
        /// 校验全部配置表 bytes 与源表一致
        /// </summary>
        public static void ValidateAll()
        {
            try
            {
                string sourceDir = ConfigImporter.SourceDir;
                string bytesDir = ConfigImporter.BytesDir;

                if (!Directory.Exists(sourceDir))
                {
                    GameLog.Error("Source excel dir missing: " + sourceDir);

                    return;
                }

                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.TopDirectoryOnly);
                int tables = 0;
                int errors = 0;
                var report = new StringBuilder();

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLowerInvariant();

                    if (ext != ".xlsx" && ext != ".xls")
                    {
                        continue;
                    }

                    string tableName = Path.GetFileNameWithoutExtension(file);
                    string abs = Path.GetFullPath(file);
                    string[][] rows = ExcelReader.ReadTable(abs);

                    if (rows == null)
                    {
                        report.AppendLine($"[ERROR] {tableName}: failed to read source table");
                        errors++;
                        continue;
                    }

                    AnalyzedTable analyzed = FieldAnalyzer.Analyze(tableName, abs, rows);

                    if (analyzed == null)
                    {
                        report.AppendLine($"[ERROR] {tableName}: failed to analyze source table");
                        errors++;
                        continue;
                    }

                    string dataPath = Path.Combine(bytesDir, tableName + ".bytes");

                    if (!File.Exists(dataPath))
                    {
                        report.AppendLine($"[MISSING] {tableName}: bytes not found under {bytesDir}");
                        errors++;
                        continue;
                    }

                    byte[] data = File.ReadAllBytes(dataPath);
                    int err = CompareTable(analyzed, data, report);
                    errors += err;
                    tables++;

                    if (err == 0)
                    {
                        report.AppendLine($"[OK] {tableName}: {analyzed.Rows.Length - analyzed.DataStartRow} rows");
                    }
                }

                if (errors == 0)
                {
                    GameLog.Info($"Config validate passed. Tables: {tables}\n{report}");
                }
                else
                {
                    GameLog.Error($"Config validate failed. Errors: {errors}\n{report}");
                }
            }
            catch (Exception error)
            {
                GameLog.Error(error);
            }
        }



        /// <summary>
        /// 比对单表 bytes 与源数据
        /// </summary>
        private static int CompareTable(AnalyzedTable table, byte[] data, StringBuilder report)
        {
            int errors = 0;
            int pos = 0;
            int magic = ReadInt32(data, ref pos);

            if (magic != Invariable.ConfigFormat.Magic)
            {
                report.AppendLine($"[{table.TableName}] magic mismatch: expected CFGT, got 0x{magic:X8}");

                return 1;
            }

            int schemaHash = ReadInt32(data, ref pos);
            int expectedHash = ConfigSchemaHash.Compute(table);

            if (schemaHash != expectedHash)
            {
                report.AppendLine($"[{table.TableName}] schemaHash mismatch: bytes=0x{schemaHash:X8} expected=0x{expectedHash:X8}");
                errors++;
            }

            int count = ReadInt32(data, ref pos);
            var ids = new int[count];

            for (int i = 0; i < count; i++)
            {
                ids[i] = ReadInt32(data, ref pos);
            }

            int expectedRows = table.Rows.Length - table.DataStartRow;

            if (count != expectedRows)
            {
                report.AppendLine($"[{table.TableName}] row count mismatch: bytes={count} excel={expectedRows}");

                return 1;
            }

            int rowSize = ReadInt32(data, ref pos);
            int dataStart = 16 + 4 * count;
            int stringRegionStart = dataStart + rowSize * count;
            var stringCache = new Dictionary<int, string>();

            for (int i = 0; i < count; i++)
            {
                int excelRow = table.DataStartRow + i;
                int excelId = int.Parse(table.Rows[excelRow][0], CultureInfo.InvariantCulture);

                if (ids[i] != excelId)
                {
                    report.AppendLine($"[{table.TableName}] id order mismatch at index {i}: bytes={ids[i]} excel={excelId}");
                    errors++;
                }

                int cursor = dataStart + i * rowSize;

                foreach (ConfigField field in table.Fields)
                {
                    if (field.ArrayLength > 0)
                    {
                        for (int a = 0; a < field.ArrayLength; a++)
                        {
                            string expected = FieldAnalyzer.Cell(table.Rows, excelRow, field.SourceColumns[a]);

                            if (!CompareScalar(field.Primitive, expected, data, ref cursor, stringRegionStart, stringCache, out string actual))
                            {
                                report.AppendLine($"[{table.TableName}] Id={excelId} field={field.Name}[{a}] expected='{expected}' actual='{actual}'");
                                errors++;
                            }
                        }
                    }
                    else
                    {
                        string expected = FieldAnalyzer.Cell(table.Rows, excelRow, field.SourceColumns[0]);

                        if (!CompareScalar(field.Primitive, expected, data, ref cursor, stringRegionStart, stringCache, out string actual))
                        {
                            report.AppendLine($"[{table.TableName}] Id={excelId} field={field.Name} expected='{expected}' actual='{actual}'");
                            errors++;
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// 比对单个标量字段
        /// </summary>
        private static bool CompareScalar(ConfigPrimitive primitive, string expectedCell, byte[] data, ref int cursor, int stringRegionStart, Dictionary<int, string> stringCache, out string actual)
        {
            expectedCell = expectedCell ?? "";

            switch (primitive)
            {
                case ConfigPrimitive.Int:
                    int expectedInt = string.IsNullOrWhiteSpace(expectedCell) ? 0 : int.Parse(expectedCell, CultureInfo.InvariantCulture);
                    int intValue = ReadInt32(data, ref cursor);
                    actual = intValue.ToString(CultureInfo.InvariantCulture);

                    return intValue == expectedInt;

                case ConfigPrimitive.Float:
                    float expectedFloat = string.IsNullOrWhiteSpace(expectedCell) ? 0f : float.Parse(expectedCell, CultureInfo.InvariantCulture);
                    float floatValue = ReadSingle(data, ref cursor);
                    actual = floatValue.ToString("G9", CultureInfo.InvariantCulture);

                    return Mathf.Abs(floatValue - expectedFloat) <= 0.0001f;

                case ConfigPrimitive.String:
                    int offset = ReadInt32(data, ref cursor);

                    if (!stringCache.TryGetValue(offset, out string stringValue))
                    {
                        int stringPos = stringRegionStart + offset;
                        stringValue = ReadStringRecord(data, ref stringPos);
                        stringCache[offset] = stringValue;
                    }

                    actual = stringValue;

                    return stringValue == expectedCell;

                default:
                    actual = "";

                    return false;
            }
        }

        /// <summary>
        /// 读取 Int32 并推进游标
        /// </summary>
        private static int ReadInt32(byte[] buffer, ref int pos)
        {
            int value = BitConverter.ToInt32(buffer, pos);
            pos += 4;

            return value;
        }

        /// <summary>
        /// 读取 Single 并推进游标
        /// </summary>
        private static float ReadSingle(byte[] buffer, ref int pos)
        {
            float value = BitConverter.ToSingle(buffer, pos);
            pos += 4;

            return value;
        }

        /// <summary>
        /// 读取对齐后的字符串记录
        /// </summary>
        private static string ReadStringRecord(byte[] buffer, ref int pos)
        {
            int charCount = ReadInt32(buffer, ref pos);

            if (charCount == 0)
            {
                Align4(ref pos);

                return "";
            }

            string text = Encoding.Unicode.GetString(buffer, pos, charCount * 2);
            pos += charCount * 2;
            Align4(ref pos);

            return text;
        }

        /// <summary>
        /// 将游标对齐到 4 字节边界
        /// </summary>
        private static void Align4(ref int pos)
        {
            int rem = pos & 3;

            if (rem != 0)
            {
                pos += 4 - rem;
            }
        }
    }
}