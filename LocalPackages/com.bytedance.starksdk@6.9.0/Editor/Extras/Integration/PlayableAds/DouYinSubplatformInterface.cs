#if TUANJIE_1_5_OR_NEWER && PLATFORM_PLAYABLEADS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace TTSDK.Tool
{
    [InitializeOnLoad]
    public static class DouYinPlayableAdsSubTargetManager
    {
        static DouYinPlayableAdsSubTargetManager()
        {
            MiniGameSubplatformManager.RegisterSubplatform(new DouYinPlayableAdsSubplatformInterface());
        }
    }

    public class DouYinPlayableAdsSubplatformInterface : DouYinSubplatformInterface
    {
        public override bool SupportsMiniGamePlatform() => false;
        public override bool SupportsPlayableAdsPlatform() => true;
        
        public override string GetSubplatformName()
        {
            return "DouYin:抖音试玩广告";
        }

        public override MiniGameSettings GetSubplatformSettings()
        {
            return new DouYinMiniGameSettings(new DouYinMiniGameSettingsEditor());
        }

        public override BuildMiniGameError Build(BuildProfile buildProfile)
        {
            // 1.Pre-processing
            string buildProfilePath = AssetDatabase.GetAssetPath(buildProfile); // Save the path of the buildProfile for post-processing
            string buildPath = buildProfile.buildPath;
            var douYinMiniGameSettings = buildProfile.miniGameSettings as DouYinMiniGameSettings;
            if (douYinMiniGameSettings is null)
            {
                Debug.LogError("预处理阶段 BuildProfile 不合法");
                return BuildMiniGameError.InvalidInput;
            }
            
            PlayerSettings playerSettings = AssetDatabase.LoadAssetAtPath<PlayerSettings>("ProjectSettings/ProjectSettings.asset"); // Global PlayerSettings
            if (buildProfile.HasOverrridePlayersettings())
                playerSettings = buildProfile.playerSettings; // Override PlayerSettings

            if (!IsBuildSettingsValid(douYinMiniGameSettings, playerSettings))
            {
                return BuildMiniGameError.InvalidInput;
            }
            
            // Playable Ads Settings
            douYinMiniGameSettings.isOldBuildFormat = false;
            douYinMiniGameSettings.needCompress = true;
            
            // 2.BuildPlayer
            SyncBuildSettingsToStark(douYinMiniGameSettings);
            var res = API.BuildManager.BuildForTuanjie(buildPath, playerSettings);
            if (string.IsNullOrEmpty(res))
            {
                return BuildMiniGameError.SubplatformConvertFailed;
            }

            // 3.Post-processing
            if (string.IsNullOrEmpty(buildProfilePath))
            {
                Debug.LogError("后处理阶段 BuildProfile 路径为空");
                return BuildMiniGameError.InvalidInput;
            }

            BuildProfile reloadBuildProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(buildProfilePath);
            if (reloadBuildProfile is null)
            {
                Debug.LogError($"后处理阶段无法重新加载 BuildProfile: {buildProfilePath}");
                return BuildMiniGameError.InvalidInput;
            }

            douYinMiniGameSettings = reloadBuildProfile.miniGameSettings as DouYinMiniGameSettings;
            if (douYinMiniGameSettings is null)
            {
                Debug.LogError("后处理阶段 BuildProfile 不合法");
                return BuildMiniGameError.InvalidInput;
            }
            
            Debug.Log("构建成功: " + res);
            return BuildMiniGameError.Succeeded;
        }

        public override BuildMiniGameError Build(BuildProfile buildProfile, BuildOptions options)
        {
            SyncBuildOptionsToStark(options);
            return Build(buildProfile);
        }
    }
}
#endif