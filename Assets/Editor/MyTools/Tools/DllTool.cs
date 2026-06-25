using UnityEditor;
using Invariable;
using System.IO;
using HybridCLR.Editor.Commands;
using System.Collections.Generic;



namespace MyTools
{
    public class DllTool
    {
        [MenuItem("VastStarryRiver/DLL/导出所有DLL", false, 0)]
        public static void BuildHotUpdateDLL()
        {
            PrebuildCommand.GenerateAll();
        }

        [MenuItem("VastStarryRiver/DLL/复制热更新DLL", false, 1)]
        public static void MoveHotUpdateDLL()
        {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string path = ConfigUtils.m_localRootPath + "HybridCLRData/HotUpdateDlls/" + platform + "/HotUpdate.dll";
            byte[] bytes = File.ReadAllBytes(path);
            ConfigUtils.SaveSafeFile(bytes, ConfigUtils.m_hotUpdateDllPath + "/" + platform + "/HotUpdate.dll.bin");
            AssetDatabase.Refresh();
        }

        [MenuItem("VastStarryRiver/DLL/复制元数据DLL", false, 2)]
        public static void MoveMetadataForAOTDLL()
        {
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();

            List<string> aotDllList = new List<string>
            {
                "mscorlib",
                "System",
                "System.Core",
                "Newtonsoft.Json",
            };

            foreach (string aotDllName in aotDllList)
            {
                string path = $"{ConfigUtils.m_localRootPath}HybridCLRData/AssembliesPostIl2CppStrip/{platform}/{aotDllName}.dll";
                byte[] bytes = File.ReadAllBytes(path);
                ConfigUtils.SaveSafeFile(bytes, $"{ConfigUtils.m_hotUpdateDllPath}/{platform}/{aotDllName}.dll.bin");
            }

            AssetDatabase.Refresh();
        }
    }
}