using Invariable;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;



namespace MyTools
{
    public class AssetBundleTool
    {
        private static readonly string PackageName = AssetBundleCollectorSettingData.Setting.Packages[0].PackageName;
        private static readonly string PipelineName = nameof(EBuildPipeline.ScriptableBuildPipeline);
        private static readonly BuildTarget BuildTarget = EditorUserBuildSettings.activeBuildTarget;



        /// <summary>
        /// 构建 AssetBundle
        /// </summary>
        [MenuItem("VastStarryRiver/构建AssetBundle", false, 20)]
        public static void BuildAssetBundle()
        {
            ExecuteBuild();
        }

        /// <summary>
        /// 获取最新资源输出路径
        /// </summary>
        /// <returns>最新资源输出路径</returns>
        public static string GetOutPath()
        {
            string buildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            string bundleParent = Path.Combine(buildOutputRoot, BuildTarget.ToString(), PackageName);
            long packageVersion = 0;

            DirectoryInfo directoryInfo = new DirectoryInfo(bundleParent);
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

            foreach (DirectoryInfo item in directoryInfos)
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
                catch (Exception error)
                {
                    _ = error;
                    continue;
                }
            }

            string path = Path.Combine(bundleParent, packageVersion.ToString());

            return path;
        }



        /// <summary>
        /// 执行构建
        /// </summary>
        private static void ExecuteBuild()
        {
            EFileNameStyle fileNameStyle = AssetBundleBuilderSetting.GetPackageFileNameStyle(PackageName, PipelineName);
            EBuildinFileCopyOption buildinFileCopyOption = AssetBundleBuilderSetting.GetPackageBuildinFileCopyOption(PackageName, PipelineName);
            string buildinFileCopyParams = AssetBundleBuilderSetting.GetPackageBuildinFileCopyParams(PackageName, PipelineName);
            ECompressOption compressOption = AssetBundleBuilderSetting.GetPackageCompressOption(PackageName, PipelineName);
            bool clearBuildCache = AssetBundleBuilderSetting.GetPackageClearBuildCache(PackageName, PipelineName);
            bool useAssetDependencyDB = AssetBundleBuilderSetting.GetPackageUseAssetDependencyDB(PackageName, PipelineName);

            ScriptableBuildParameters buildParameters = new ScriptableBuildParameters();
            buildParameters.BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot();
            buildParameters.BuildPipeline = PipelineName;
            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;
            buildParameters.BuildTarget = BuildTarget;
            buildParameters.PackageName = PackageName;
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
                GameLog.Info("YooAsset Build Success！");
            }
            else
            {
                GameLog.Error("YooAsset Build Fail！");
            }
        }

        /// <summary>
        /// 创建资源包加密服务类实例
        /// </summary>
        private static IEncryptionServices CreateEncryptionServicesInstance()
        {
            string className = AssetBundleBuilderSetting.GetPackageEncyptionServicesClassName(PackageName, PipelineName);
            List<Type> classTypes = EditorTools.GetAssignableTypes(typeof(IEncryptionServices));
            Type classType = classTypes.Find((x) => x.FullName.Equals(className));

            if (classType != null)
            {
                return (IEncryptionServices)Activator.CreateInstance(classType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 创建资源清单加密服务类实例
        /// </summary>
        private static IManifestProcessServices CreateManifestProcessServicesInstance()
        {
            string className = AssetBundleBuilderSetting.GetPackageManifestProcessServicesClassName(PackageName, PipelineName);
            List<Type> classTypes = EditorTools.GetAssignableTypes(typeof(IManifestProcessServices));
            Type classType = classTypes.Find((x) => x.FullName.Equals(className));

            if (classType != null)
            {
                return (IManifestProcessServices)Activator.CreateInstance(classType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 创建资源清单解密服务类实例
        /// </summary>
        private static IManifestRestoreServices CreateManifestRestoreServicesInstance()
        {
            string className = AssetBundleBuilderSetting.GetPackageManifestRestoreServicesClassName(PackageName, PipelineName);
            List<Type> classTypes = EditorTools.GetAssignableTypes(typeof(IManifestRestoreServices));
            Type classType = classTypes.Find((x) => x.FullName.Equals(className));

            if (classType != null)
            {
                return (IManifestRestoreServices)Activator.CreateInstance(classType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string GetBuiltinShaderBundleName()
        {
            bool uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            PackRuleResult packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();

            return packRuleResult.GetBundleName(PackageName, uniqueBundleName);
        }

        /// <summary>
        /// 获取默认版本
        /// </summary>
        private static string GetDefaultPackageVersion()
        {
            return $"{DateTime.Now.Year}{DateTime.Now.ToString("MMddHHmmss")}";
        }
    }
}