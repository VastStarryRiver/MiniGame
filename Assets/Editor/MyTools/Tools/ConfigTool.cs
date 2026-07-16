using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExcelDataReader;
using Invariable;



namespace MyTools
{
    public class ConfigTool
    {
        /// <summary>
        /// 配置字段信息
        /// </summary>
        private struct ConfigFieldInfo
        {
            public string fieldName;
            public string csharpType;
        }



        [MenuItem("VastStarryRiver/Config/导出Web配置", false, 0)]
        public static void BuildWebBinFile()
        {
            using (FileStream fileStream = new FileStream(ConfigUtils.m_localRootPath + "WebData.txt", FileMode.Open))
            {
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    ConfigUtils.SaveSafeFile(streamReader.ReadToEnd(), ConfigUtils.m_localResourcePath + "/WebData.bin");
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("VastStarryRiver/Config/导出Excel配置", false, 1)]
        public static void ExportConfig()
        {
            if (Directory.Exists(ConfigUtils.m_configCsPath))
            {
                Directory.Delete(ConfigUtils.m_configCsPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_configCsPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(ConfigUtils.m_configExcelPath);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (var item in fileInfos)
            {
                string excelPath = item.FullName.Replace("\\", "/");

                using (FileStream fileStream = new FileStream(excelPath, FileMode.Open))
                {
                    IExcelDataReader excelReader = null;

                    if (excelPath.EndsWith(".xls"))
                    {
                        excelReader = ExcelReaderFactory.CreateBinaryReader(fileStream);
                    }
                    else if (excelPath.EndsWith(".xlsx"))
                    {
                        excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);
                    }

                    if (excelReader != null)
                    {
                        //判断Excel文件中是否存在至少一张数据表
                        if (excelReader.ResultsCount > 0)
                        {
                            LoadExcelRowData(excelReader);
                        }

                        excelReader.Dispose();
                        excelReader.Close();
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
        }



        private static void LoadExcelRowData(IExcelDataReader excelReader)
        {
            var clientColumnIndex = new List<int>();
            var fieldNameList = new List<string>();
            var fieldNoteList = new List<string>();
            var dataTypeList = new List<string>();
            var clientConfigData = new Dictionary<string, Dictionary<string, string>>();

            do
            {
                clientColumnIndex.Clear();
                fieldNameList.Clear();
                dataTypeList.Clear();
                clientConfigData.Clear();

                int keyIndex = 1;

                while (excelReader.Read()/*下一行*/)
                {
                    List<string> columnData = LoadExcelColumnData(excelReader);

                    if (excelReader.Depth == 0)
                    {
                        fieldNoteList = columnData;
                        continue;
                    }
                    else if (excelReader.Depth == 1)
                    {
                        for (int i = 1; i < columnData.Count; i++)
                        {
                            clientColumnIndex.Add(i);
                        }

                        continue;
                    }
                    else if (excelReader.Depth == 2)
                    {
                        fieldNameList = columnData;

                        for (int i = 0; i < columnData.Count; i++)
                        {
                            if (columnData[i] == "Index")
                            {
                                keyIndex = i;
                                break;
                            }
                        }

                        continue;
                    }
                    else if (excelReader.Depth == 3)
                    {
                        dataTypeList = columnData;
                        continue;
                    }

                    if (columnData[0] == "NO")
                    {
                        continue;
                    }

                    for (int i = 1; i < columnData.Count; i++)
                    {
                        if (clientColumnIndex.Contains(i))
                        {
                            LoadExcelData(fieldNameList[i], columnData[i], ref clientConfigData, columnData[keyIndex]);
                        }
                    }

                    EditorUtility.DisplayProgressBar("配置表" + excelReader.Name + "正在导出数据中", "导出进度" + (excelReader.Depth + 1) + "/" + excelReader.RowCount, (excelReader.Depth + 1) * 1.0f / excelReader.RowCount);

                    if (columnData[0] == "END")
                    {
                        goto over;
                    }
                }

            over:;

                SaveConfigCode(excelReader.Name, clientConfigData, fieldNameList, dataTypeList, clientColumnIndex, fieldNoteList);
            }
            while (excelReader.NextResult()/*下一张表*/);
        }



        private static List<string> LoadExcelColumnData(IExcelDataReader excelReader)
        {
            List<string> columnData = new List<string>();

            for (int i = 0; i < excelReader.FieldCount; i++)
            {
                string value;

                try
                {
                    value = excelReader.GetString(i);
                }
                catch
                {
                    value = excelReader.GetDouble(i).ToString();
                }

                columnData.Add(value);
            }

            return columnData;
        }

        private static void LoadExcelData(string name, string data, ref Dictionary<string, Dictionary<string, string>> content, string key)
        {
            if (!content.ContainsKey(key))
            {
                content[key] = new Dictionary<string, string>();
            }

            content[key][name] = data;
        }

        /// <summary>
        /// 将配置表保存为可直接运行的 C# 脚本
        /// 数据直接嵌入到 .cs 文件中，不再依赖 .bin
        /// </summary>
        private static void SaveConfigCode(string sheetName, Dictionary<string, Dictionary<string, string>> configData, List<string> fieldNameList, List<string> dataTypeList, List<int> clientColumnIndex, List<string> fieldNoteList)
        {
            if (configData.Count <= 0)
            {
                return;
            }

            string className = GetValidIdentifier(sheetName);
            string filePath = ConfigUtils.m_configCsPath + "/Tab_" + className + ".cs";

            var clientFields = GetClientFieldInfos(fieldNameList, dataTypeList, clientColumnIndex);

            if (clientFields.Count <= 0)
            {
                return;
            }

            // 判断 Index 列的类型
            string indexType = "string";
            for (int i = 0; i < fieldNameList.Count; i++)
            {
                if (fieldNameList[i] == "Index" && i < dataTypeList.Count)
                {
                    indexType = GetCSharpType(dataTypeList[i]);
                    fieldNoteList.RemoveAt(i);
                    fieldNoteList.RemoveAt(0);
                    break;
                }
            }
            bool indexIsInt = indexType == "int";

            // 移除 clientFields 中重复的 Index（Index 会单独处理）
            var dataFields = new List<ConfigFieldInfo>();
            for (int i = 0; i < clientFields.Count; i++)
            {
                if (clientFields[i].fieldName != "Index")
                {
                    dataFields.Add(clientFields[i]);
                }
            }

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("using System;");
            stringBuilder.AppendLine("using System.Collections.Generic;");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("namespace HotUpdate");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("    public static class Tab_" + className);
            stringBuilder.AppendLine("    {");

            // 行数据类
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 配置表行数据");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        [System.Serializable]");
            stringBuilder.AppendLine("        public class Row");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            public " + (indexIsInt ? "int" : "string") + " Index; // 索引");

            for (int i = 0; i < dataFields.Count; i++)
            {
                string fieldName = dataFields[i].fieldName;
                string fieldType = dataFields[i].csharpType;
                stringBuilder.AppendLine("            public " + fieldType + " " + fieldName + "; // " + fieldNoteList[i]);
            }

            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // 静态数据容器
            stringBuilder.AppendLine("        private static List<Row> s_configs; // 所有配置行");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("        private static Dictionary<string, Row> s_configDict; // 索引到行的快速查找");
            stringBuilder.AppendLine();

            // Init
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 预加载配置表（数据已嵌入 .cs，调用即完成初始化）");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        /// <param name=\"onComplete\">加载完成回调</param>");
            stringBuilder.AppendLine("        public static void Init(Action onComplete = null)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            if (s_configs != null)");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                onComplete?.Invoke();");
            stringBuilder.AppendLine("                return;");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("            BuildConfigs();");
            stringBuilder.AppendLine("            onComplete?.Invoke();");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // GetConfigByIndex (int)
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 根据整数索引获取一行配置");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        /// <param name=\"index\">行索引</param>");
            stringBuilder.AppendLine("        /// <returns>配置行数据，不存在返回 null</returns>");
            stringBuilder.AppendLine("        public static Row GetConfigByIndex(int index)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            return GetConfigByIndex(index.ToString());");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // GetConfigByIndex (string)
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 根据字符串索引获取一行配置");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        /// <param name=\"index\">行索引</param>");
            stringBuilder.AppendLine("        /// <returns>配置行数据，不存在返回 null</returns>");
            stringBuilder.AppendLine("        public static Row GetConfigByIndex(string index)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            if (s_configDict == null)");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                BuildConfigs();");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("            if (s_configDict != null && s_configDict.TryGetValue(index, out Row row))");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                return row;");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("            return null;");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // GetAllConfigs
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 获取所有配置行");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        /// <returns>配置行列表</returns>");
            stringBuilder.AppendLine("        public static List<Row> GetAllConfigs()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            if (s_configs == null)");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                BuildConfigs();");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("            return s_configs ?? new List<Row>();");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // Count
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 配置表总行数");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        public static int Count");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            get");
            stringBuilder.AppendLine("            {");
            stringBuilder.AppendLine("                if (s_configs == null)");
            stringBuilder.AppendLine("                {");
            stringBuilder.AppendLine("                    BuildConfigs();");
            stringBuilder.AppendLine("                }");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("                return s_configs != null ? s_configs.Count : 0;");
            stringBuilder.AppendLine("            }");
            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // BuildConfigs
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 手动构建全部配置行（数据直接嵌入在 .cs 中）");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        private static void BuildConfigs()");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            s_configs = new List<Row>();");
            stringBuilder.AppendLine("            s_configDict = new Dictionary<string, Row>();");
            stringBuilder.AppendLine();

            foreach (var item in configData)
            {
                string key = item.Key;
                Dictionary<string, string> rowData = item.Value;

                stringBuilder.Append("            AddRow(new Row { ");

                // Index
                if (indexIsInt && int.TryParse(key, out int intKey))
                {
                    stringBuilder.Append("Index = " + intKey);
                }
                else
                {
                    stringBuilder.Append("Index = " + FormatStringLiteral(key));
                }

                // Data fields
                for (int i = 0; i < dataFields.Count; i++)
                {
                    string fieldName = dataFields[i].fieldName;
                    string fieldType = dataFields[i].csharpType;

                    if (!rowData.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    string value = rowData[fieldName];
                    string literal = FormatValueLiteral(value, fieldType);

                    stringBuilder.Append(", " + fieldName + " = " + literal);
                }

                stringBuilder.AppendLine(" });");
            }

            stringBuilder.AppendLine("        }");
            stringBuilder.AppendLine();

            // AddRow
            stringBuilder.AppendLine("        /// <summary>");
            stringBuilder.AppendLine("        /// 添加一行配置到容器中");
            stringBuilder.AppendLine("        /// </summary>");
            stringBuilder.AppendLine("        private static void AddRow(Row row)");
            stringBuilder.AppendLine("        {");
            stringBuilder.AppendLine("            string key = row.Index.ToString();");
            stringBuilder.AppendLine("            s_configs.Add(row);");
            stringBuilder.AppendLine("            s_configDict[key] = row;");
            stringBuilder.AppendLine("        }");

            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("}");

            ConfigUtils.CreateFileByBytes(filePath, Encoding.UTF8.GetBytes(stringBuilder.ToString()));

            AssetDatabase.Refresh();
        }



        /// <summary>
        /// 将 Excel 字段值格式化为 C# 字面量
        /// </summary>
        private static string FormatValueLiteral(string value, string csharpType)
        {
            if (value == null)
            {
                value = "";
            }

            switch (csharpType)
            {
                case "int":
                    if (int.TryParse(value, out int intResult))
                    {
                        return intResult.ToString();
                    }
                    return "0";

                case "float":
                    if (float.TryParse(value, out float floatResult))
                    {
                        return floatResult.ToString("G9") + "f";
                    }
                    return "0f";

                case "bool":
                    {
                        string v = value.Trim().ToLower();
                        return v == "1" || v == "true" || v == "yes" ? "true" : "false";
                    }

                case "int[]":
                    return FormatIntArrayLiteral(value);

                case "float[]":
                    return FormatFloatArrayLiteral(value);

                case "string[]":
                    return FormatStringArrayLiteral(value);

                case "string":
                default:
                    return FormatStringLiteral(value);
            }
        }



        /// <summary>
        /// 字符串字面量转义
        /// </summary>
        private static string FormatStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append('\"');

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }

            sb.Append('\"');
            return sb.ToString();
        }



        /// <summary>
        /// int[] 字面量
        /// </summary>
        private static string FormatIntArrayLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "new int[0]";
            }

            string[] parts = value.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.Append("new int[] { ");

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                if (int.TryParse(parts[i].Trim(), out int result))
                {
                    sb.Append(result);
                }
                else
                {
                    sb.Append("0");
                }
            }

            sb.Append(" }");
            return sb.ToString();
        }



        /// <summary>
        /// float[] 字面量
        /// </summary>
        private static string FormatFloatArrayLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "new float[0]";
            }

            string[] parts = value.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.Append("new float[] { ");

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                if (float.TryParse(parts[i].Trim(), out float result))
                {
                    sb.Append(result.ToString("G9"));
                    sb.Append("f");
                }
                else
                {
                    sb.Append("0f");
                }
            }

            sb.Append(" }");
            return sb.ToString();
        }



        /// <summary>
        /// string[] 字面量
        /// </summary>
        private static string FormatStringArrayLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "new string[0]";
            }

            string[] parts = value.Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            sb.Append("new string[] { ");

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(FormatStringLiteral(parts[i].Trim()));
            }

            sb.Append(" }");
            return sb.ToString();
        }



        /// <summary>
        /// 获取客户端字段信息列表
        /// </summary>
        private static List<ConfigFieldInfo> GetClientFieldInfos(List<string> fieldNameList, List<string> dataTypeList, List<int> clientColumnIndex)
        {
            var result = new List<ConfigFieldInfo>();

            for (int i = 0; i < clientColumnIndex.Count; i++)
            {
                int col = clientColumnIndex[i];

                if (col < 0 || col >= fieldNameList.Count || col >= dataTypeList.Count)
                {
                    continue;
                }

                string rawFieldName = fieldNameList[col];
                string fieldName = GetValidIdentifier(rawFieldName);

                if (string.IsNullOrEmpty(fieldName))
                {
                    fieldName = "Field" + col;
                }

                result.Add(new ConfigFieldInfo
                {
                    fieldName = fieldName,
                    csharpType = GetCSharpType(dataTypeList[col])
                });
            }

            return result;
        }



        /// <summary>
        /// Excel 类型转 C# 类型
        /// </summary>
        private static string GetCSharpType(string excelType)
        {
            if (string.IsNullOrEmpty(excelType))
            {
                return "string";
            }

            string type = excelType.Trim().ToLower();

            switch (type)
            {
                case "int":
                case "int32":
                    return "int";

                case "float":
                case "single":
                case "double":
                    return "float";

                case "bool":
                case "boolean":
                    return "bool";

                case "string":
                case "str":
                    return "string";

                case "int[]":
                case "list<int>":
                    return "int[]";

                case "float[]":
                case "list<float>":
                    return "float[]";

                case "string[]":
                case "list<string>":
                    return "string[]";

                default:
                    return "string";
            }
        }



        /// <summary>
        /// 将字段名或表名转换为合法 C# 标识符
        /// </summary>
        private static string GetValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];

                if (char.IsLetter(c) || c == '_')
                {
                    stringBuilder.Append(c);
                }
                else if (char.IsDigit(c))
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.Append('_');
                    }

                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append('_');
                }
            }

            string result = stringBuilder.ToString();

            if (IsCSharpKeyword(result))
            {
                result = "@" + result;
            }

            return result;
        }



        /// <summary>
        /// 判断是否为 C# 关键字
        /// </summary>
        private static bool IsCSharpKeyword(string word)
        {
            switch (word)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    return true;

                default:
                    return false;
            }
        }
    }
}
