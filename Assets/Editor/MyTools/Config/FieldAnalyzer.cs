using Invariable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;



namespace MyTools
{
    public enum ConfigPrimitive
    {
        Int,
        Float,
        String
    }

    public sealed class ConfigField
    {
        public string Name;
        public ConfigPrimitive Primitive;
        public int ArrayLength;
        public int[] SourceColumns;
        public string CsType;
        public int WireSize;
        public string Comment;
    }

    public sealed class AnalyzedTable
    {
        public string TableName;
        public string SourcePath;
        public List<ConfigField> Fields = new List<ConfigField>();
        public string[][] Rows;
        public int DataStartRow = 3;
    }

    public static class FieldAnalyzer
    {
        private static readonly Regex NameNumber = new Regex(@"^(.*?)(\d+)$", RegexOptions.Compiled);
        private static readonly HashSet<string> CsKeywords = new HashSet<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while"
        };



        /// <summary>
        /// 分析表头与类型，生成字段布局
        /// </summary>
        public static AnalyzedTable Analyze(string tableName, string sourcePath, string[][] rows)
        {
            if (rows == null || rows.Length < 4)
            {
                GameLog.Error($"Table too short: {sourcePath}");

                return null;
            }

            string[] names = rows[0];
            string[] types = rows[1];
            string[] comments = rows.Length > 2 ? rows[2] : Array.Empty<string>();
            names[0] = "Id";

            var raw = new List<(string name, string type, int col, string baseName, int number, string comment)>();

            for (int col = 0; col < names.Length; col++)
            {
                string name = (names[col] ?? "").Trim();

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                string type = (col < types.Length ? types[col] : "int")?.Trim().ToLowerInvariant() ?? "int";
                string comment = col < comments.Length ? (comments[col] ?? "") : "";
                string baseName = name;
                int number = -1;
                Match match = NameNumber.Match(name);

                if (match.Success && name != "Id")
                {
                    baseName = match.Groups[1].Value;
                    number = int.Parse(match.Groups[2].Value);
                }

                raw.Add((name, type, col, baseName, number, comment));
            }

            raw.Sort((a, b) =>
            {
                int cmp = string.CompareOrdinal(a.baseName, b.baseName);

                if (cmp != 0)
                {
                    return cmp;
                }

                return a.number.CompareTo(b.number);
            });

            var fields = new List<ConfigField>();

            for (int i = 0; i < raw.Count;)
            {
                (string name, string type, int col, string baseName, int number, string comment) current = raw[i];

                if (current.number >= 0)
                {
                    var cols = new List<int> { current.col };
                    string type = current.type;
                    string comment = current.comment;
                    int next = i + 1;

                    while (next < raw.Count && raw[next].number >= 0 && raw[next].baseName == current.baseName)
                    {
                        if (raw[next].type != type)
                        {
                            GameLog.Error($"Array column type mismatch: {current.baseName} in {sourcePath}");

                            return null;
                        }

                        cols.Add(raw[next].col);
                        next++;
                    }

                    ConfigField arrayField = CreateField(current.baseName, type, cols.ToArray(), comment);

                    if (arrayField == null)
                    {
                        return null;
                    }

                    fields.Add(arrayField);
                    i = next;
                }
                else
                {
                    ConfigField field = CreateField(current.name, current.type, new[] { current.col }, current.comment);

                    if (field == null)
                    {
                        return null;
                    }

                    fields.Add(field);
                    i++;
                }
            }

            if (!fields.Exists((field) => field.Name == "Id"))
            {
                GameLog.Error($"[{tableName}] 缺少 Id 字段。Path:{sourcePath}");

                return null;
            }

            if (!ValidateFieldNames(tableName, sourcePath, fields))
            {
                return null;
            }

            if (!ValidateIds(tableName, sourcePath, rows))
            {
                return null;
            }

            int idIndex = fields.FindIndex((field) => field.Name == "Id");

            if (idIndex > 0)
            {
                ConfigField idField = fields[idIndex];
                fields.RemoveAt(idIndex);
                fields.Insert(0, idField);
            }

            if (fields[0].Primitive != ConfigPrimitive.Int || fields[0].ArrayLength > 0)
            {
                GameLog.Error("Id must be scalar int: " + sourcePath);

                return null;
            }

            return new AnalyzedTable
            {
                TableName = tableName,
                SourcePath = sourcePath,
                Fields = fields,
                Rows = rows
            };
        }

        /// <summary>
        /// 安全读取单元格文本
        /// </summary>
        public static string Cell(string[][] rows, int row, int col)
        {
            if (row < 0 || row >= rows.Length)
            {
                return "";
            }

            string[] line = rows[row];

            if (col < 0 || col >= line.Length)
            {
                return "";
            }

            return line[col] ?? "";
        }



        /// <summary>
        /// 创建单个配置字段描述
        /// </summary>
        private static ConfigField CreateField(string name, string type, int[] cols, string comment)
        {
            ConfigPrimitive? primitive = ParsePrimitive(type);

            if (primitive == null)
            {
                return null;
            }

            string csType;
            int wire;
            int arrayLength = cols.Length > 1 ? cols.Length : 0;

            switch (primitive.Value)
            {
                case ConfigPrimitive.Int:
                    csType = arrayLength > 0 ? "int[]" : "int";
                    wire = 4 * (arrayLength > 0 ? arrayLength : 1);
                    break;

                case ConfigPrimitive.Float:
                    csType = arrayLength > 0 ? "float[]" : "float";
                    wire = 4 * (arrayLength > 0 ? arrayLength : 1);
                    break;

                case ConfigPrimitive.String:
                    csType = arrayLength > 0 ? "string[]" : "string";
                    wire = 4 * (arrayLength > 0 ? arrayLength : 1);
                    break;

                default:
                    GameLog.Error("Unknown type");

                    return null;
            }

            return new ConfigField
            {
                Name = name,
                Primitive = primitive.Value,
                ArrayLength = arrayLength,
                SourceColumns = cols,
                CsType = csType,
                WireSize = wire,
                Comment = comment ?? ""
            };
        }

        /// <summary>
        /// 解析原始类型字符串
        /// </summary>
        private static ConfigPrimitive? ParsePrimitive(string type)
        {
            switch (type)
            {
                case "int":
                    return ConfigPrimitive.Int;

                case "float":
                    return ConfigPrimitive.Float;

                case "string":
                    return ConfigPrimitive.String;

                default:
                    GameLog.Error("Unsupported type: " + type);

                    return null;
            }
        }

        /// <summary>
        /// 校验字段名合法、非关键字且不重名
        /// </summary>
        private static bool ValidateFieldNames(string tableName, string path, List<ConfigField> fields)
        {
            var names = new HashSet<string>();

            for (int i = 0; i < fields.Count; i++)
            {
                string fieldName = fields[i].Name;

                if (!IsValidIdentifier(fieldName))
                {
                    GameLog.Error($"[{tableName}] 字段名非法（须为合法 C# 标识符）: {fieldName}。Path:{path}");

                    return false;
                }

                if (CsKeywords.Contains(fieldName))
                {
                    GameLog.Error($"[{tableName}] 字段名不能使用 C# 关键字: {fieldName}。Path:{path}");

                    return false;
                }

                if (!names.Add(fieldName))
                {
                    GameLog.Error($"[{tableName}] 字段名重复: {fieldName}。Path:{path}");

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断是否为合法 C# 标识符
        /// </summary>
        public static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (!(char.IsLetter(name[0]) || name[0] == '_'))
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!(char.IsLetterOrDigit(name[i]) || name[i] == '_'))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 校验 Id 列可解析且不重复
        /// </summary>
        private static bool ValidateIds(string tableName, string path, string[][] rows)
        {
            var set = new HashSet<int>();

            for (int row = 3; row < rows.Length; row++)
            {
                if (rows[row].Length == 0)
                {
                    continue;
                }

                if (!int.TryParse(rows[row][0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    GameLog.Error($"[{tableName}] 第 {row + 1} 行, 字段 \"Id\"(int): 值 \"{rows[row][0]}\" 解析失败。Path:{path}");

                    return false;
                }

                if (!set.Add(id))
                {
                    GameLog.Error($"[{tableName}] 第 {row + 1} 行, 字段 \"Id\"(int): 值 \"{id}\" 重复。Path:{path}");

                    return false;
                }
            }

            return true;
        }
    }
}