using UnityEditor;
using System.Collections.Generic;
using System.IO;
using ExcelDataReader;
using Invariable;



namespace MyTools
{
    public class ConfigTool
    {
        [MenuItem("VastStarryRiver/Config/导出Web配置", false, 10)]
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

        [MenuItem("VastStarryRiver/Config/导出Excel配置", false, 11)]
        public static void ExportConfig()
        {
            if (Directory.Exists(ConfigUtils.m_configBinPath))
            {
                Directory.Delete(ConfigUtils.m_configBinPath, true);
            }

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
            var serverColumnIndex = new List<int>();
            var fieldNameList = new List<string>();
            var dataTypeList = new List<string>();
            var clientConfigData = new Dictionary<string, Dictionary<string, string>>();
            var serverConfigData = new Dictionary<string, Dictionary<string, string>>();

            do
            {
                clientColumnIndex.Clear();
                serverColumnIndex.Clear();
                fieldNameList.Clear();
                dataTypeList.Clear();
                clientConfigData.Clear();
                serverConfigData.Clear();

                string key = "";
                int keyIndex = 1;

                Dictionary<string, int> configKey = new Dictionary<string, int>();

                while (excelReader.Read()/*下一行*/)
                {
                    List<string> columnData = LoadExcelColumnData(excelReader);

                    if (excelReader.Depth == 0)
                    {
                        continue;
                    }
                    else if (excelReader.Depth == 1)
                    {
                        for (int i = 0; i < columnData.Count; i++)
                        {
                            if (columnData[i] == "1")
                            {
                                clientColumnIndex.Add(i);
                            }
                            else if (columnData[i] == "2")
                            {
                                serverColumnIndex.Add(i);
                            }
                            else if (columnData[i] == "3")
                            {
                                clientColumnIndex.Add(i);
                                serverColumnIndex.Add(i);
                            }
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
                            LoadExcelData(dataTypeList[i], fieldNameList[i], columnData[i], ref clientConfigData, columnData[keyIndex]);
                        }

                        if (serverColumnIndex.Contains(i))
                        {
                            LoadExcelData(dataTypeList[i], fieldNameList[i], columnData[i], ref serverConfigData, columnData[keyIndex]);
                        }
                    }

                    if (columnData[0] == "END")
                    {
                        goto over;
                    }

                    EditorUtility.DisplayProgressBar("配置表" + excelReader.Name + "正在导出数据中", "导出进度" + (excelReader.Depth + 1) + "/" + excelReader.RowCount, (excelReader.Depth + 1) * 1.0f / excelReader.RowCount);
                }

            over:;

                SaveConfigData("Client", excelReader.Name, clientConfigData);
                //SaveConfigData("Server", excelReader.Name, serverConfigData);
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

        private static void LoadExcelData(string type, string name, string data, ref Dictionary<string, Dictionary<string, string>> content, string key)
        {
            if (!content.ContainsKey(key))
            {
                content[key] = new Dictionary<string, string>();
            }

            content[key][name] = data;
        }

        private static void SaveConfigData(string platform, string name, Dictionary<string, Dictionary<string, string>> configData)
        {
            if (configData.Count <= 0)
            {
                return;
            }

            ConfigUtils.SaveSafeFile(configData, ConfigUtils.m_configBinPath + "/" + name + ".bin");

            AssetDatabase.Refresh();
        }
    }
}