using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;



public class AssetBundleTool
{
    private readonly static string packageName = AssetBundleCollectorSettingData.Setting.Packages[0].PackageName;
    private readonly static string pipelineName = nameof(EBuildPipeline.ScriptableBuildPipeline);
    private readonly static BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;



    [MenuItem("VastStarryRiver/构建AssetBundle", false, 20)]
    public static void BuildAssetBundle()
    {
        ExecuteBuild();
    }



    /// <summary>
    /// 执行构建
    /// </summary>
    private static void ExecuteBuild()
    {
        var fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(packageName, pipelineName);
        var buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(packageName, pipelineName);
        var buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(packageName, pipelineName);
        var compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(packageName, pipelineName);
        var clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(packageName, pipelineName);
        var useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(packageName, pipelineName);

        ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
        buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
        buildParameters.BuildPipeline = pipelineName;
        buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
        buildParameters.BuildTarget = buildTarget;
        buildParameters.PackageName = packageName;
        buildParameters.PackageVersion = GetDefaultPackageVersion();
        buildParameters.EnableSharePackRule = true;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.FileNameStyle = fileNameStyle;
        buildParameters.BuildinFileCopyOption = buildinFileCopyOption;
        buildParameters.BuildinFileCopyParams = buildinFileCopyParams;
        buildParameters.CompressOption = compressOption;
        buildParameters.ClearBuildCacheFiles = clearBuildCache;
        buildParameters.UseAssetDependencyDB = useAssetDependencyDB;
        buildParameters.EncryptionServices = CreateEncryptionServicesInstance();
        buildParameters.ManifestProcessServices = CreateManifestProcessServicesInstance();
        buildParameters.ManifestRestoreServices = CreateManifestRestoreServicesInstance();
        buildParameters.BuiltinShadersBundleName = GetBuiltinShaderBundleName();

        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        BuildResult buildResult = pipeline.Run(buildParameters, true);

        if (buildResult.Success)
        {
            Debug.Log("YooAsset Build Success！");
        }
        else
        {
            Debug.LogError("YooAsset Build Fail！");
        }
    }

    /// <summary>
    /// 创建资源包加密服务类实例
    /// </summary>
    private static IEncryptionServices CreateEncryptionServicesInstance()
    {
        var className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(packageName, pipelineName);
        var classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
        var classType = classTypes.Find(x => x.FullName.Equals(className));
        if (classType != null)
            return (IEncryptionServices)Activator.CreateInstance(classType);
        else
            return null;
    }

    /// <summary>
    /// 创建资源清单加密服务类实例
    /// </summary>
    private static IManifestProcessServices CreateManifestProcessServicesInstance()
    {
        var className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(packageName, pipelineName);
        var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
        var classType = classTypes.Find(x => x.FullName.Equals(className));
        if (classType != null)
            return (IManifestProcessServices)Activator.CreateInstance(classType);
        else
            return null;
    }

    /// <summary>
    /// 创建资源清单解密服务类实例
    /// </summary>
    private static IManifestRestoreServices CreateManifestRestoreServicesInstance()
    {
        var className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(packageName, pipelineName);
        var classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
        var classType = classTypes.Find(x => x.FullName.Equals(className));
        if (classType != null)
            return (IManifestRestoreServices)Activator.CreateInstance(classType);
        else
            return null;
    }

    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName()
    {
        var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(packageName, uniqueBundleName);
    }

    /// <summary>
    /// 获取默认版本
    /// </summary>
    private static string GetDefaultPackageVersion()
    {
        return $"{DateTime.Now.Year}{DateTime.Now.ToString("MMddHHmmss")}";
    }

    /// <summary>
    /// 获取最新资源输出路径
    /// </summary>
    /// <returns></returns>
    public static string GetOutPath()
    {
        string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
        string bundleParent = Path.Combine(buildOutputRoot, buildTarget.ToString(), packageName);
        long packageVersion = 0;

        DirectoryInfo directoryInfo = new DirectoryInfo(bundleParent);
        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

        foreach (var item in directoryInfos)
        {
            try
            {
                long version = long.Parse(item.Name);

                if (packageVersion > 0)
                {
                    if (version > packageVersion)
                    {
                        packageVersion = version;
                    }
                }
                else
                {
                    packageVersion = version;
                }
            }
            catch (Exception _)
            {
                continue;
            }
        }

        string path = Path.Combine(bundleParent, packageVersion.ToString());
        return path;
    }
}