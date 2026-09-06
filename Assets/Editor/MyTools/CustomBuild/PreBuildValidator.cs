using Invariable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;



namespace MyTools
{
    public static class PreBuildValidator
    {
        public enum MiniGamePackTarget
        {
            WeChat,
            DouYin
        }

        private const string CloudHelperRelativePath = "Assets/Scripts/CloudService/CloudHelper.cs";
        private const string EditorBuildSettingsRelativePath = "ProjectSettings/EditorBuildSettings.asset";
        private const string MiniGameConfigRelativePath = "Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset";
        private const string StarkBuilderRelativePath = "Assets/Editor/StarkBuilderSetting.asset";
        private const string WeChatProfileAssetPath = "Assets/Settings/Build Profiles/WeChat Profile.asset";
        private const string DouYinProfileAssetPath = "Assets/Settings/Build Profiles/DouYin Profile.asset";
        private static readonly string[] SecretFieldNames =
        {
            "GameId",
            "WechatAppId",
            "WechatAppSecret",
            "DouyinAppId",
            "DouyinAppSecret"
        };



        /// <summary>
        /// 打包前校验，硬条件失败或用户未确认远程模式时返回 false
        /// </summary>
        public static bool ConfirmReadyToPack(MiniGamePackTarget target)
        {
            try
            {
                List<string> issues = CollectIssues(target);

                if (issues.Count > 0)
                {
                    string detail = string.Join("；", issues);
                    GameLog.Error($"打包前检查未通过：{detail}");
                    EditorUtility.DisplayDialog("打包前检查未通过", detail, "确定");

                    return false;
                }

                bool confirmed = EditorUtility.DisplayDialog(
                    "确认远程调用模式",
                    "工程内无法验证 Func Stateless 是否已切远程调用。请先在 UOS/Func Stateless 面板确认远程模式后再继续打包。",
                    "已确认远程模式，继续",
                    "取消");

                if (!confirmed)
                {
                    GameLog.Info("用户取消打包，未确认远程调用模式");

                    return false;
                }

                return true;
            }
            catch (Exception error)
            {
                string detail = $"检查脚本自身出错：{error.Message}";
                GameLog.Error($"打包前检查未通过：{detail}");
                EditorUtility.DisplayDialog("打包前检查未通过", detail, "确定");

                return false;
            }
        }



        /// <summary>
        /// 收集 CDN、Secrets、Profile、平台宏与成对配置问题
        /// </summary>
        private static List<string> CollectIssues(MiniGamePackTarget target)
        {
            List<string> issues = new List<string>();

            if (IsPlaceholder(InvariableConst.CDNPathWeChat))
            {
                issues.Add("CDNPathWeChat 为空或仍为占位符");
            }

            if (IsPlaceholder(InvariableConst.CDNPathDouYin))
            {
                issues.Add("CDNPathDouYin 为空或仍为占位符");
            }

            string helperText = ReadProjectText(CloudHelperRelativePath);

            if (helperText == null)
            {
                issues.Add("找不到 CloudHelper.cs，无法检查 Secrets");
            }
            else
            {
                foreach (string fieldName in SecretFieldNames)
                {
                    string value = ReadConstValue(helperText, fieldName);

                    if (IsPlaceholder(value))
                    {
                        issues.Add($"CloudHelper.Secrets.{fieldName} 未填或仍为占位符");
                    }
                }
            }

#if MINIGAME_SUBPLATFORM_WEIXIN && MINIGAME_SUBPLATFORM_DOUYIN
            issues.Add("同一批工程设置里同时出现微信与抖音平台宏");

#elif MINIGAME_SUBPLATFORM_WEIXIN
            if (target == MiniGamePackTarget.DouYin)
            {
                issues.Add("打包抖音小游戏时当前子平台为微信");
            }

#elif MINIGAME_SUBPLATFORM_DOUYIN
            if (target == MiniGamePackTarget.WeChat)
            {
                issues.Add("打包微信小游戏时当前子平台为抖音");
            }

#else
            issues.Add("当前未激活微信或抖音子平台");
#endif

            string profileText = ReadProjectText(EditorBuildSettingsRelativePath);

            if (profileText == null)
            {
                issues.Add("找不到 EditorBuildSettings.asset，无法检查 Build Profile");
            }
            else
            {
                bool weChatEnabled = IsProfileEnabled(profileText, WeChatProfileAssetPath);
                bool douYinEnabled = IsProfileEnabled(profileText, DouYinProfileAssetPath);

                if (weChatEnabled && douYinEnabled)
                {
                    issues.Add("微信与抖音 Build Profile 同时启用，平台宏不唯一");
                }
                else if (target == MiniGamePackTarget.WeChat && !weChatEnabled)
                {
                    issues.Add("打包微信小游戏时 WeChat Profile 未启用");
                }
                else if (target == MiniGamePackTarget.DouYin && !douYinEnabled)
                {
                    issues.Add("打包抖音小游戏时 DouYin Profile 未启用");
                }
            }

            AddPairedConfigIssues(issues, target);

            return issues;
        }

        /// <summary>
        /// 比较 SDK 配置与 Build Profile 的成对字段，仅目标平台硬拦
        /// </summary>
        private static void AddPairedConfigIssues(List<string> issues, MiniGamePackTarget target)
        {
            List<string> weChatIssues = new List<string>();
            List<string> douYinIssues = new List<string>();

            AddWeChatPairIssues(weChatIssues);
            AddDouYinPairIssues(douYinIssues);

            if (target == MiniGamePackTarget.WeChat)
            {
                issues.AddRange(weChatIssues);
                LogNonTargetIssues(douYinIssues, "抖音");
            }
            else
            {
                issues.AddRange(douYinIssues);
                LogNonTargetIssues(weChatIssues, "微信");
            }
        }

        /// <summary>
        /// 比较微信 MiniGameConfig 与 WeChat Profile 的 Appid、MemorySize、Orientation、输出路径
        /// </summary>
        private static void AddWeChatPairIssues(List<string> issues)
        {
            string miniGameText = ReadProjectText(MiniGameConfigRelativePath);
            string profileText = ReadProjectText(WeChatProfileAssetPath);

            if (miniGameText == null)
            {
                issues.Add("找不到 MiniGameConfig.asset，无法检查微信平台配置配对");
            }

            if (profileText == null)
            {
                issues.Add("找不到 WeChat Profile.asset，无法检查微信平台配置配对");
            }

            if (miniGameText == null || profileText == null)
            {
                return;
            }

            string settingsBlock = ReadYamlClassScope(profileText, "WeixinMiniGameSettings");
            string topLevel = ReadYamlTopLevelScope(profileText);

            if (settingsBlock == null)
            {
                issues.Add("WeChat Profile 找不到 WeixinMiniGameSettings 块，无法检查微信平台配置配对");
            }
            else
            {
                ComparePairedScalar(issues, miniGameText, settingsBlock, "Appid", "Appid", "微信 Appid");
                ComparePairedScalar(issues, miniGameText, settingsBlock, "MemorySize", "MemorySize", "微信 MemorySize");
                ComparePairedScalar(issues, miniGameText, settingsBlock, "Orientation", "Orientation", "微信 Orientation");
            }

            if (string.IsNullOrWhiteSpace(topLevel))
            {
                issues.Add("WeChat Profile 找不到顶层区，无法检查输出路径配对");
            }
            else
            {
                ComparePairedScalar(issues, miniGameText, topLevel, "DST", "m_BuildPath", "微信输出路径", true);
            }
        }

        /// <summary>
        /// 比较抖音 StarkBuilderSetting 与 DouYin Profile 的 appId、内存、方向、输出路径
        /// </summary>
        private static void AddDouYinPairIssues(List<string> issues)
        {
            string starkText = ReadProjectText(StarkBuilderRelativePath);
            string profileText = ReadProjectText(DouYinProfileAssetPath);

            if (starkText == null)
            {
                issues.Add("找不到 StarkBuilderSetting.asset，无法检查抖音平台配置配对");
            }

            if (profileText == null)
            {
                issues.Add("找不到 DouYin Profile.asset，无法检查抖音平台配置配对");
            }

            if (starkText == null || profileText == null)
            {
                return;
            }

            string settingsBlock = ReadYamlClassScope(profileText, "DouYinMiniGameSettings");
            string topLevel = ReadYamlTopLevelScope(profileText);

            if (settingsBlock == null)
            {
                issues.Add("DouYin Profile 找不到 DouYinMiniGameSettings 块，无法检查抖音平台配置配对");
            }
            else
            {
                ComparePairedScalar(issues, starkText, settingsBlock, "_appId", "appId", "抖音 AppID");
                ComparePairedScalar(issues, starkText, settingsBlock, "wasmMemorySize", "wasmMemorySize", "抖音 wasmMemorySize");
                ComparePairedScalar(issues, starkText, settingsBlock, "_orientation", "orientation", "抖音 orientation");
            }

            if (string.IsNullOrWhiteSpace(topLevel))
            {
                issues.Add("DouYin Profile 找不到顶层区，无法检查输出路径配对");
            }
            else
            {
                ComparePairedScalar(issues, starkText, topLevel, "OutputDir", "m_BuildPath", "抖音输出路径", true);
            }
        }

        /// <summary>
        /// 非目标平台的配对问题只记日志，不阻断当前打包
        /// </summary>
        private static void LogNonTargetIssues(List<string> issues, string platformName)
        {
            if (issues == null || issues.Count == 0)
            {
                return;
            }

            string detail = string.Join("；", issues);
            GameLog.Info($"打包前检查：{platformName} 平台配置配对有问题，但不阻断当前目标打包：{detail}");
        }

        /// <summary>
        /// 取首个 type class 标记之前的顶层区，没有标记则返回全文
        /// </summary>
        private static string ReadYamlTopLevelScope(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            Match first = Regex.Match(text, @"(?m)^\s*type:\s*\{class:");
            if (!first.Success)
            {
                return text;
            }

            return text.Substring(0, first.Index);
        }

        /// <summary>
        /// 取指定 class 名的 YAML 块，下一块 type class 为边界，找不到返回 null
        /// </summary>
        private static string ReadYamlClassScope(string text, string className)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            Match start = Regex.Match(text, @"(?m)^\s*type:\s*\{class:\s*" + Regex.Escape(className) + @"\s*,");
            if (!start.Success)
            {
                return null;
            }

            int begin = start.Index;
            string rest = text.Substring(begin + start.Length);
            Match next = Regex.Match(rest, @"(?m)^\s*type:\s*\{class:");
            int end = next.Success ? begin + start.Length + next.Index : text.Length;

            return text.Substring(begin, end - begin);
        }

        /// <summary>
        /// 比较两侧 YAML 标量，缺字段、歧义或值不一致时记入 issues
        /// </summary>
        private static void ComparePairedScalar(List<string> issues, string leftText, string rightText, string leftField, string rightField, string label, bool isPath = false)
        {
            string leftValue = ReadYamlScalar(leftText, leftField, out bool leftAmbiguous);
            string rightValue = ReadYamlScalar(rightText, rightField, out bool rightAmbiguous);

            if (leftAmbiguous || rightAmbiguous)
            {
                issues.Add($"{label} 在配置中出现多次，无法确定权威值");

                return;
            }

            if (leftValue == null || rightValue == null)
            {
                issues.Add($"{label} 字段读取失败，可能 SDK 已改名，无法确认两侧一致");

                return;
            }

            if (!AreConfigValuesEqual(leftValue, rightValue, isPath))
            {
                issues.Add($"{label} 两侧不一致");
            }
        }

        /// <summary>
        /// 读取 YAML 单行标量，零个返回 null，多于一个标歧义
        /// </summary>
        private static string ReadYamlScalar(string text, string fieldName, out bool ambiguous)
        {
            ambiguous = false;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            MatchCollection matches = Regex.Matches(text, @"(?m)^\s*" + Regex.Escape(fieldName) + @":\s*(.*)$");
            if (matches.Count == 0)
            {
                return null;
            }

            if (matches.Count > 1)
            {
                ambiguous = true;

                return null;
            }

            return matches[0].Groups[1].Value.Trim();
        }

        /// <summary>
        /// 比较配置值，路径比较忽略分隔符与末尾斜杠
        /// </summary>
        private static bool AreConfigValuesEqual(string left, string right, bool isPath)
        {
            if (isPath)
            {
                string normalizedLeft = left.Replace('\\', '/').TrimEnd('/');
                string normalizedRight = right.Replace('\\', '/').TrimEnd('/');

                return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(left.Trim(), right.Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// 空值或 {占位符} 视为未配置
        /// </summary>
        private static bool IsPlaceholder(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            return Regex.IsMatch(value, @"\{[^}]+\}");
        }

        /// <summary>
        /// 从源码读取 Name = "value" 的首个赋值
        /// </summary>
        private static string ReadConstValue(string source, string name)
        {
            Match match = Regex.Match(source, name + @"\s*=\s*""([^""]*)""");

            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null;
        }

        /// <summary>
        /// 按项目根相对路径读取文本，文件不存在返回 null
        /// </summary>
        private static string ReadProjectText(string relativePath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string fullPath = Path.Combine(projectRoot, relativePath);

            if (!File.Exists(fullPath))
            {
                return null;
            }

            return File.ReadAllText(fullPath);
        }

        /// <summary>
        /// 解析 EditorBuildSettings 中指定 Profile 是否 enabled
        /// </summary>
        private static bool IsProfileEnabled(string buildSettingsText, string profileAssetPath)
        {
            string pattern = @"enabled:\s*1\s*\r?\n\s*path: " + Regex.Escape(profileAssetPath);

            return Regex.IsMatch(buildSettingsText, pattern);
        }
    }
}