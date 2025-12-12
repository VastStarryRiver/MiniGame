using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeChatWASM;



public class CustomBuildScript
{
    [MenuItem("VastStarryRiver/打包/打包微信小游戏", false, 30)]
    public static void PackageProject_WeiXin()
    {
        if (Directory.Exists(ConfigUtils.m_miniBuildPath))
        {
            Directory.Delete(ConfigUtils.m_miniBuildPath, true);
        }

        ConfigUtils.InitDirectory(ConfigUtils.m_miniBuildPath);

        if (WXConvertCore.DoExport() == WXConvertCore.WXExportError.SUCCEED)
        {
            if (WXConvertCore.IsInstantGameAutoStreaming())
            {
                if (!string.IsNullOrEmpty(WXConvertCore.FirstBundlePath) && File.Exists(WXConvertCore.FirstBundlePath))
                {
                    Debug.Log("转换成功");
                }
                else
                {
                    Debug.LogError("转换失败");
                }
            }
        }
    }

    [MenuItem("VastStarryRiver/打包/复制文件到CDN目录", false, 31)]
    public static void MoveFileToCND()
    {
        if (Directory.Exists(ConfigUtils.m_cdnPath))
        {
            Directory.Delete(ConfigUtils.m_cdnPath, true);
        }

        ConfigUtils.InitDirectory(ConfigUtils.m_cdnPath);

        MoveBundleToCND();
        MoveMiniGameToCND();
    }



    private static void MoveBundleToCND()
    {
        string path = AssetBundleTool.GetOutPath();

        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        FileInfo[] fileInfos = directoryInfo.GetFiles();

        foreach (var item in fileInfos)
        {
            string sourceFilePath = item.FullName.Replace("\\", "/");
            string targetFilePath = ConfigUtils.m_cdnPath + "/" + Path.GetFileName(sourceFilePath);
            File.Copy(sourceFilePath, targetFilePath);
        }
    }

    private static void MoveMiniGameToCND()
    {
        string path = ConfigUtils.m_miniWebglPath;

        if (!Directory.Exists(path))
        {
            return;
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        FileInfo[] fileInfos = directoryInfo.GetFiles();

        foreach (var item in fileInfos)
        {
            if (!item.FullName.Contains(".webgl.data.unityweb.bin.br"))
            {
                continue;
            }

            string sourceFilePath = item.FullName.Replace("\\", "/");
            string targetFilePath = ConfigUtils.m_cdnPath + "/" + Path.GetFileName(sourceFilePath);
            File.Copy(sourceFilePath, targetFilePath);

            break;
        }
    }
}