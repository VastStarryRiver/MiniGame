using Invariable;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

#if MINIGAME_SUBPLATFORM_WEIXIN
using WeChatWASM;

#elif MINIGAME_SUBPLATFORM_DOUYIN
using TTSDK.Tool;
#endif



namespace MyTools
{
    public class CustomBuildScript
    {
        /// <summary>
        /// 打包微信小游戏
        /// </summary>
        [MenuItem("VastStarryRiver/打包/打包微信小游戏", false, 30)]
        public static void PackageProject_WeiXin()
        {
            string path = $"{ConfigUtils.MiniBuildPath}/WeChat";

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            ConfigUtils.InitDirectory(path);

#if MINIGAME_SUBPLATFORM_WEIXIN
            if (WXConvertCore.DoExport() == WXConvertCore.WXExportError.SUCCEED)
            {
                if (WXConvertCore.IsInstantGameAutoStreaming())
                {
                    if (!string.IsNullOrEmpty(WXConvertCore.FirstBundlePath) && File.Exists(WXConvertCore.FirstBundlePath))
                    {
                        GameLog.Info("微信小游戏构建完成！");
                    }
                    else
                    {
                        GameLog.Error("微信小游戏构建失败");
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 微信小游戏菜单是否可用
        /// </summary>
        [MenuItem("VastStarryRiver/打包/打包微信小游戏", true, 30)]
        public static bool PackageProject_WeiXin_Enable()
        {
#if MINIGAME_SUBPLATFORM_WEIXIN
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 打包抖音小游戏
        /// </summary>
        [MenuItem("VastStarryRiver/打包/打包抖音小游戏", false, 31)]
        public static void PackageProject_DouYin()
        {
            string path = $"{ConfigUtils.MiniBuildPath}/DouYin";

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            ConfigUtils.InitDirectory(path);

#if MINIGAME_SUBPLATFORM_DOUYIN
            BuildProfile buildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/DouYin Profile.asset");
            DouYinSubplatformInterface douYinSubplatformInterface = new DouYinSubplatformInterface();
            douYinSubplatformInterface.Build(buildProfile, BuildOptions.None);
#endif
        }

        /// <summary>
        /// 抖音小游戏菜单是否可用
        /// </summary>
        [MenuItem("VastStarryRiver/打包/打包抖音小游戏", true, 31)]
        public static bool PackageProject_DouYin_Enable()
        {
#if MINIGAME_SUBPLATFORM_DOUYIN
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 复制最新 bundle 到 CDN 目录
        /// </summary>
        [MenuItem("VastStarryRiver/打包/复制bundle到CDN目录", false, 32)]
        public static void MoveBundleFileToCDN()
        {
            string path2 = ConfigUtils.CdnPath + "/yoo";

            if (Directory.Exists(path2))
            {
                Directory.Delete(path2, true);
            }

            ConfigUtils.InitDirectory(path2);

            string path = AssetBundleTool.GetOutPath();

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (FileInfo item in fileInfos)
            {
                string sourceFilePath = item.FullName.Replace("\\", "/");
                string targetFilePath = $"{path2}/{Path.GetFileName(sourceFilePath)}";
                File.Copy(sourceFilePath, targetFilePath);
            }
        }

        /// <summary>
        /// 复制 unityweb.bin 到 CDN 目录
        /// </summary>
        [MenuItem("VastStarryRiver/打包/复制unityweb.bin到CDN目录", false, 33)]
        public static void MoveCodeFileToCDN()
        {
            string path = "";

#if MINIGAME_SUBPLATFORM_WEIXIN
            path = $"{ConfigUtils.MiniBuildPath}/WeChat/webgl";

#elif MINIGAME_SUBPLATFORM_DOUYIN
            path = $"{ConfigUtils.MiniBuildPath}/DouYin/webgl";
#endif

            if (!Directory.Exists(path))
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles();

            foreach (FileInfo item in fileInfos)
            {
                if (!item.FullName.Contains(".webgl.data.unityweb.bin.br") && !item.FullName.Contains(".webgl.data.unityweb.bin.txt"))
                {
                    continue;
                }

                string sourceFilePath = item.FullName.Replace("\\", "/");
                string targetFilePath = $"{ConfigUtils.CdnPath}/{Path.GetFileName(sourceFilePath)}";
                File.Copy(sourceFilePath, targetFilePath);

                break;
            }
        }
    }
}