using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;
using Invariable;

#if MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK.Tool;
#endif



namespace MyTools
{
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

#if MINIGAME_SUBPLATFORM_WEIXIN
            if (WXConvertCore.DoExport() == WXConvertCore.WXExportError.SUCCEED)
            {
                if (WXConvertCore.IsInstantGameAutoStreaming())
                {
                    if (!string.IsNullOrEmpty(WXConvertCore.FirstBundlePath) && File.Exists(WXConvertCore.FirstBundlePath))
                    {
                        Debug.Log("微信小游戏构建完成！");
                    }
                    else
                    {
                        Debug.LogError("微信小游戏构建失败");
                    }
                }
            }
#endif
        }

        [MenuItem("VastStarryRiver/打包/打包微信小游戏", true, 30)]
        public static bool PackageProject_WeiXin_Enable()
        {
#if MINIGAME_SUBPLATFORM_WEIXIN
            return true;
#else
            return false;
#endif
        }

        [MenuItem("VastStarryRiver/打包/复制文件到CDN目录", false, 31)]
        public static void MoveFileToCND()
        {
            if (Directory.Exists(ConfigUtils.m_cdnPath))
            {
                Directory.Delete(ConfigUtils.m_cdnPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_cdnPath + "/yoo");

            MoveBundleToCND();

#if MINIGAME_SUBPLATFORM_WEIXIN
            MoveMiniGameToCND();
#endif
        }

        [MenuItem("VastStarryRiver/打包/打包抖音小游戏", false, 32)]
        public static void PackageProject_DouYin()
        {
            if (Directory.Exists(ConfigUtils.m_miniBuildPath))
            {
                Directory.Delete(ConfigUtils.m_miniBuildPath, true);
            }

            ConfigUtils.InitDirectory(ConfigUtils.m_miniBuildPath);

#if MINIGAME_SUBPLATFORM_DOUYIN
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/DouYin Profile.asset");
            DouYinSubplatformInterface douYinSubplatformInterface = new DouYinSubplatformInterface();
            douYinSubplatformInterface.Build(buildProfile, BuildOptions.None);
#endif
        }

        [MenuItem("VastStarryRiver/打包/打包抖音小游戏", true, 32)]
        public static bool PackageProject_DouYin_Enable()
        {
#if MINIGAME_SUBPLATFORM_DOUYIN
            return true;
#else
            return false;
#endif
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
                string targetFilePath = ConfigUtils.m_cdnPath + "/yoo/" + Path.GetFileName(sourceFilePath);
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
                if (!item.FullName.Contains(".webgl.data.unityweb.bin.br") && !item.FullName.Contains(".webgl.data.unityweb.bin.txt"))
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
}