# 构建、热更新与平台适配

## 1. 平台概览

项目同时支持：

- 微信小游戏；
- 抖音小游戏；
- 编辑器模拟模式。

平台条件宏：

```csharp
MINIGAME_SUBPLATFORM_WEIXIN
MINIGAME_SUBPLATFORM_DOUYIN
```

平台文件系统还要求：

```csharp
UNITY_WEBGL
```

因此小游戏运行时通常需要同时满足：

```text
UNITY_WEBGL + 对应 MINIGAME_SUBPLATFORM 宏
```

当前 Build Profile：

```text
Assets/Settings/Build Profiles/
├─ WeChat Profile.asset
└─ DouYin Profile.asset
```

当前 `EditorBuildSettings.asset` 中微信 Profile 为启用状态，抖音 Profile 为未启用状态。切平台时应通过团结引擎 Build Profile 正确激活，而不是只改宏文本。

> Profile 中包含 AppID、CDN、绝对构建路径等环境相关信息。文档不重复记录具体值；修改或分享时应注意凭据和环境隔离。

## 2. `SdkManager` 平台能力

文件：

```text
Assets/Scripts/Invariable/Manager/SdkManager.cs
```

统一封装：

| 能力 | Editor | 微信 | 抖音 |
|---|---|---|---|
| SDK 初始化 | 立即回调 | `WX.InitSDK` | `TT.InitSDK` |
| 平台登录 | 返回 null | `WX.Login` | `TT.Login`（`forceLogin=true`） |
| 小游戏版本更新 | 无 | `WXUpdateManager` | `TTUpdateManager` |
| 本地字符串存储 | PlayerPrefs | WX Storage | TT Save |
| 云存档读写入口 | 走本地存储 | 转发 `CloudManager` 云缓存 | 转发 `CloudManager` 云缓存 |
| 原生键盘 | 无平台调用 | WX Keyboard | TT Keyboard |
| 方向变化 | 直接适配 | WX 监听 | TT 监听 |
| 激励视频 | 直接回调成功 | 已有框架 | 已有框架 |
| 侧边栏复访 | 无 | 无 | 有（跳转成功写入本地 IsGetReward=1） |
| 游戏圈按钮 | 无 | 有 | 无 |
| 分享 | 输出日志 | WX.ShareAppMessage | TT.ShareAppMessage（成功/失败/取消回调） |
| 环境判断 | IsWeChat/IsDouYin 均返回 false | IsWeChat 返回 true | IsDouYin 返回 true |
| YooAsset 文件系统 | 不走此接口 | 微信 FS | 抖音 FS |

## 3. YooAsset 小游戏平台文件系统

### 3.1 微信

创建位置：

```csharp
string packageRoot =
    $"{WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
```

文件系统：

```text
WechatFileSystem
```

主要行为：

- 使用 `WXFileSystemManager`；
- 下载远程 Bundle 到微信缓存；
- 通过 `WX.GetCachePath` 判断缓存；
- 支持清理全部或未使用 Bundle；
- 创建时检查远程 URL 是否含双斜杠；
- 远程根地址来自 `WebData.bin` 中 CDN 地址的 `/yoo` 子路径。

### 3.2 抖音

创建位置：

```csharp
string packageRoot = "yoo";
```

文件系统：

```text
TiktokFileSystem
```

主要行为：

- 使用 `TTFileSystemManager`；
- 通过 URL 缓存接口判断和加载；
- 远程根地址同样来自 CDN 的 `/yoo` 子路径。

## 4. WebData 生成

### 4.1 源文件

项目根目录手工创建：

```text
WebData.txt
```

最少需要第一行 CDN 根地址。代码还支持后续行（当前框架未使用）：

```text
第 1 行：CDN 根地址
第 2 行：服务器 IP:Port
第 3 行：认证用户名
第 4 行：认证密码
```

### 4.2 导出

菜单：

```text
VastStarryRiver/Config/导出Web配置
```

执行：

```text
WebData.txt
  -> ConfigUtils.SaveSafeFile
  -> Assets/Resources/LocalAssets/WebData.bin
  -> BinImporter
  -> BinAsset
```

运行时：

```text
Resources.Load("LocalAssets/WebData")
  -> 解密
  -> ConfigUtils.SetWebData
  -> ConfigUtils.CDNPath
```

### 4.3 注意

- `WebData.bin` 属于首包本地资源，修改 CDN 后需要重新构建基础包；
- 末尾换行和 Windows `\r\n` 已通过读取时移除 `\r` 部分处理。

## 5. Excel 配置导出

菜单：

```text
VastStarryRiver/Config/导出Excel配置
```

流程：

```text
Excel/*.xls[x]
  -> ExcelDataReader
  -> ConfigTool
  -> 删除 HotUpdate/Config
  -> 生成 Tab_*.cs
  -> 编译进入 HotUpdate.dll
```

必须先导表，再生成热更新 DLL。否则 DLL 中仍是旧数据。

## 6. HybridCLR DLL 构建

编辑器菜单：

```text
VastStarryRiver/DLL/导出所有DLL
VastStarryRiver/DLL/复制热更新DLL
VastStarryRiver/DLL/复制元数据DLL
```

### 6.1 导出所有 DLL

调用：

```csharp
PrebuildCommand.GenerateAll();
```

主要输出：

```text
HybridCLRData/
├─ HotUpdateDlls/{ActiveBuildTarget}/HotUpdate.dll
└─ AssembliesPostIl2CppStrip/{ActiveBuildTarget}/*.dll
```

### 6.2 复制热更新 DLL

读取：

```text
HybridCLRData/HotUpdateDlls/{platform}/HotUpdate.dll
```

加密输出：

```text
Assets/GameAssets/DLL/{platform}/HotUpdate.dll.bin
```

### 6.3 复制 AOT 元数据 DLL

固定列表：

```text
mscorlib
System
System.Core
Newtonsoft.Json
```

输出：

```text
Assets/GameAssets/DLL/{platform}/{AOT程序集}.dll.bin
```

### 6.4 平台名强约束

编辑器工具使用：

```csharp
EditorUserBuildSettings.activeBuildTarget.ToString()
```

运行时代码却固定调用：

```csharp
LoadMetadataForAOTAssemblies("MiniGame", ...)
```

因此当前构建目标字符串必须产生目录 `MiniGame`，资源地址才会匹配：

```text
MiniGame_HotUpdate.dll
MiniGame_System.dll
...
```

若引擎升级后 BuildTarget 名变化，需同步修改运行时或生成路径。

## 7. YooAsset 构建

菜单：

```text
VastStarryRiver/构建AssetBundle
```

实现：

```text
AssetBundleTool.cs
```

参数：

- Pipeline：`ScriptableBuildPipeline`
- Package：`MyPackage`
- Bundle 类型：AssetBundle
- BuildTarget：当前激活目标
- 版本号：`yyyyMMddHHmmss` 风格的数字字符串
- 共享打包：开启
- 构建结果校验：开启
- 资源/清单加密服务：读取 YooAsset Builder Settings

输出根目录由 YooAsset 默认 Builder 配置决定。

最新输出路径通过遍历版本目录并选择最大数字版本得到。

## 8. 复制资源到 CDN

菜单：

```text
VastStarryRiver/打包/复制bundle到CDN目录
```

流程：

1. 删除本地 `CDN/yoo`；
2. 找到 `MyPackage` 最新数字版本目录；
3. 将该目录顶层文件复制到 `CDN/yoo`。

远程目录必须满足：

```text
{ConfigUtils.CDNPath}/yoo/{YooAsset清单和Bundle文件}
```

注意：

- 工具只复制最新输出目录的顶层文件；
- 目标目录会被整个删除；
- 上传 CDN 前确认没有混入旧平台或错误版本；
- 清单和 Bundle 必须作为一个一致版本上传；
- CDN 缓存规则不能导致新清单引用尚未生效的 Bundle。

## 9. 微信小游戏构建

菜单：

```text
VastStarryRiver/打包/打包微信小游戏
```

仅在微信宏激活时可用。

核心调用：

```csharp
WXConvertCore.DoExport()
```

Build Profile：

```text
Assets/Settings/Build Profiles/WeChat Profile.asset
```

输出约定：

```text
Build/WeChat
```

Profile 中包含：

- AppID；
- CDN；
- 竖屏方向；
- WebGL2；
- 内存；
- Brotli；
- 首包/StreamingAssets 相关设置；
- 绝对输出路径。

路径是机器相关配置。迁移工程目录后应重新检查。

## 10. 抖音小游戏构建

菜单：

```text
VastStarryRiver/打包/打包抖音小游戏
```

仅在抖音宏激活时可用。

核心调用：

```csharp
BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(
    "Assets/Settings/Build Profiles/DouYin Profile.asset"
);

new DouYinSubplatformInterface()
    .Build(profile, BuildOptions.None);
```

输出约定：

```text
Build/DouYin
```

Profile/Stark 设置中包含：

- AppID；
- CDN；
- wasm 内存；
- URL 缓存白名单；
- 压缩；
- 方向；
- 绝对输出路径。

## 11. 复制代码数据文件到 CDN

菜单：

```text
VastStarryRiver/打包/复制unityweb.bin到CDN目录
```

按当前平台从：

```text
Build/WeChat/webgl
或
Build/DouYin/webgl
```

查找包含以下名称之一的文件：

```text
.webgl.data.unityweb.bin.br
.webgl.data.unityweb.bin.txt
```

复制到：

```text
CDN/
```

该步骤与 YooAsset 的 `CDN/yoo` 不同，属于平台 WebGL 数据文件发布。

## 12. 推荐的完整构建顺序

切换平台后建议按以下顺序执行：

### 12.1 首次或基础包完整发布

1. 激活目标平台 Build Profile；
2. 确认只激活正确的平台宏；
3. 检查 Profile 的 AppID、CDN、输出路径和方向；
4. 创建/更新 `WebData.txt`；
5. 导出 Web 配置；
6. 修改 Excel 后导出 Excel 配置；
7. 等待脚本编译成功；
8. 导出所有 HybridCLR DLL；
9. 复制热更新 DLL；
10. 复制 AOT 元数据 DLL；
11. 构建 YooAsset；
12. 复制 Bundle 到 `CDN/yoo`；
13. 上传/发布 CDN 内容；
14. 上传云函数（`UOS -> Func Stateless -> Open Panel`）并切换为远程调用模式；
15. 构建微信或抖音小游戏；
16. 如平台数据走 CDN，复制并上传 unityweb 数据文件；
17. 使用平台开发者工具启动；
18. 清缓存和保留缓存两种情况各测试一次；
19. 提交审核或发布。

### 12.2 仅业务代码热更新

前提：只改 `HotUpdate`，没有改 `Invariable`、平台 SDK、首场景和首包资源。

1. 确认目标平台仍正确；
2. 如改了 Excel，重新导出配置；
3. 生成 HybridCLR DLL；
4. 复制 `HotUpdate.dll.bin`；
5. 若 AOT 依赖未变化，可不一定重复制元数据，但完整流水线仍建议复制；
6. 构建 YooAsset 新版本；
7. 复制并上传 `CDN/yoo`；
8. 验证远程新清单和 Bundle；
9. 在旧基础包上启动验证热更新。

### 12.3 仅动态资源更新

1. 修改 `Assets/GameAssets`；
2. 不需要重新构建 HotUpdate DLL，除非脚本也变更；
3. 构建 YooAsset；
4. 上传最新清单和 Bundle；
5. 在旧基础包上验证下载与加载。

## 13. 什么不能只靠热更新发布

以下修改通常需要重新发布基础小游戏包：

- `Assets/Scripts/Invariable`；
- `Assets/Scripts/CloudService`（同时需重新上传云函数并确认远程调用模式）；
- `Assets/ToolPackage/YooAsset`；
- `Assets/Scenes/Start.scene`；
- `Assets/Resources/LocalAssets`；
- `WebData.bin`；
- 微信/抖音 SDK 版本；
- Build Profile 和 PlayerSettings 中影响运行时的设置；
- AOT 代码和基础程序集引用变化；
- 引擎版本；
- HybridCLR 框架配置；
- 首包加载界面或平台转换配置。

Editor 工具本身不进入运行时，但其产物变化可能要求重新构建。

## 14. 平台功能验证矩阵

每次发布建议至少验证：

| 场景 | Editor | 微信真机 | 抖音真机 |
|---|---:|---:|---:|
| 首次无缓存启动 | 模拟 | 必测 | 必测 |
| 有旧缓存启动 | 不适用 | 必测 | 必测 |
| 资源更新进度 | 模拟有限 | 必测 | 必测 |
| DLL 加载和主界面 | 必测 | 必测 | 必测 |
| 本地存档 | 必测 | 必测 | 必测 |
| 原生键盘 | 无完整模拟 | 必测 | 必测 |
| 前后台切换 | 有限 | 必测 | 必测 |
| 小游戏版本更新 | 无 | 必测 | 必测 |
| 激励视频完整/中断 | 直接回调成功 | 配置后必测 | 配置后必测 |
| 分享成功/失败/取消 | 输出日志 | 必测 | 必测 |
| 安全区/横竖屏 | 有限 | 多机型 | 多机型 |
| CDN 异常/断网 | 可模拟 | 必测 | 必测 |

## 15. 构建产物与源码的对应关系

```text
HotUpdate 源码/生成配置
  -> HybridCLR HotUpdate.dll
  -> ConfigUtils.SaveSafeFile
  -> GameAssets/DLL/MiniGame/HotUpdate.dll.bin
  -> YooAsset Bundle
  -> CDN/yoo
  -> 客户端下载
  -> 解密
  -> Assembly.Load
  -> HotUpdate.StartGame.Play
```

任一环节仍使用旧产物，都会表现为“源码已修改但真机没有变化”。排查时应逐段确认时间戳、清单版本、下载日志和缓存。