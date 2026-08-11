using ExcelDataReader;
using Invariable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;



namespace MyTools
{
    public static class ExcelReader
    {
        /// <summary>
        /// 读取源表为 string[row][col]，仅支持 .xlsx/.xls
        /// </summary>
        public static string[][] ReadTable(string absolutePath)
        {
            string ext = Path.GetExtension(absolutePath).ToLowerInvariant();

            if (ext == ".xlsx" || ext == ".xls")
            {
                return ReadXlsx(absolutePath);
            }

            GameLog.Error("Unsupported table source: " + absolutePath);

            return null;
        }

        /// <summary>
        /// 读取 Excel（.xlsx/.xls）首个工作表
        /// </summary>
        public static string[][] ReadXlsx(string absolutePath)
        {
            using (FileStream stream = File.Open(absolutePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IExcelDataReader reader;
                string ext = Path.GetExtension(absolutePath).ToLowerInvariant();

                if (ext == ".xls")
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }

                using (reader)
                {
                    var list = new List<string[]>();

                    while (reader.Read())
                    {
                        int fields = reader.FieldCount;
                        var row = new string[fields];
                        bool allEmpty = true;

                        for (int c = 0; c < fields; c++)
                        {
                            object value = null;

                            try
                            {
                                value = reader.GetValue(c);
                            }
                            catch (Exception error)
                            {
                                _ = error;
                                value = null;
                            }

                            row[c] = value == null ? "" : Convert.ToString(value, CultureInfo.InvariantCulture);

                            if (!string.IsNullOrWhiteSpace(row[c]))
                            {
                                allEmpty = false;
                            }
                        }

                        if (!allEmpty)
                        {
                            list.Add(row);
                        }
                    }

                    return list.ToArray();
                }
            }
        }
    }
}