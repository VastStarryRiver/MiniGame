using Invariable;
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
#if MINIGAME_SUBPLATFORM_WEIXIN
            if (!ApplyCDNPathToWeChatConfigs())
            {
                return;
            }
#endif
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
#if MINIGAME_SUBPLATFORM_DOUYIN
            if (!ApplyCDNPathToDouYinConfigs())
            {
                return;
            }
#endif
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



        /// <summary>
        /// 将 CDN 根地址写入微信 MiniGameConfig 与 WeChat Profile
        /// </summary>
        public static bool ApplyCDNPathToWeChatConfigs()
        {
#if MINIGAME_SUBPLATFORM_WEIXIN
            string cdnPath = InvariableConst.CDNPath;
            UnityEngine.Object miniGameConfig = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset");

            if (miniGameConfig == null)
            {
                GameLog.Error("未找到 MiniGameConfig.asset，已中止写入微信 CDN");

                return false;
            }

            SerializedObject miniGameConfigObject = new SerializedObject(miniGameConfig);
            SerializedProperty miniGameCdn = miniGameConfigObject.FindProperty("ProjectConf.CDN");

            if (miniGameCdn == null)
            {
                GameLog.Error("MiniGameConfig.asset 缺少 ProjectConf.CDN，已中止写入微信 CDN");

                return false;
            }

            miniGameCdn.stringValue = cdnPath;
            miniGameConfigObject.ApplyModifiedProperties();
            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/WeChat Profile.asset");

            if (profile == null)
            {
                GameLog.Error("未找到 WeChat Profile.asset，已中止写入微信 CDN");

                return false;
            }

            WeixinMiniGameSettings settings = profile.miniGameSettings as WeixinMiniGameSettings;

            if (settings == null || settings.ProjectConf == null)
            {
                GameLog.Error("WeChat Profile 的 MiniGameSettings 无效，已中止写入微信 CDN");

                return false;
            }

            settings.ProjectConf.CDN = cdnPath;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            return true;
#else
            GameLog.Error("当前未激活微信平台，无法写入微信 CDN 配置");

            return false;
#endif
        }

        /// <summary>
        /// 将 CDN 根地址写入抖音 Profile 与 StarkBuilderSetting
        /// </summary>
        public static bool ApplyCDNPathToDouYinConfigs()
        {
#if MINIGAME_SUBPLATFORM_DOUYIN
            string cdnPath = InvariableConst.CDNPath;
            BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>("Assets/Settings/Build Profiles/DouYin Profile.asset");

            if (profile == null)
            {
                GameLog.Error("未找到 DouYin Profile.asset，已中止写入抖音 CDN");

                return false;
            }

            DouYinMiniGameSettings settings = profile.miniGameSettings as DouYinMiniGameSettings;

            if (settings == null)
            {
                GameLog.Error("DouYin Profile 的 MiniGameSettings 无效，已中止写入抖音 CDN");

                return false;
            }

            settings.CDN = cdnPath;
            EditorUtility.SetDirty(profile);
            UnityEngine.Object starkSetting = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Editor/StarkBuilderSetting.asset");

            if (starkSetting == null)
            {
                GameLog.Error("未找到 StarkBuilderSetting.asset，已中止写入抖音 CDN");

                return false;
            }

            SerializedObject starkObject = new SerializedObject(starkSetting);
            SerializedProperty starkCdn = starkObject.FindProperty("CDN");

            if (starkCdn == null)
            {
                GameLog.Error("StarkBuilderSetting.asset 缺少 CDN，已中止写入抖音 CDN");

                return false;
            }

            starkCdn.stringValue = cdnPath;
            starkObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            return true;
#else
            GameLog.Error("当前未激活抖音平台，无法写入抖音 CDN 配置");

            return false;
#endif
        }
    }
}