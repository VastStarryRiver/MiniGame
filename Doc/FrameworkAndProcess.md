# 框架架构与运行流程

## 1. 技术栈

| 类别 | 技术/版本 | 用途 | 依赖形态 |
|---|---|---|---|
| 引擎 | 团结引擎 1.6.8 / Unity 2022.3.61t9 | 游戏运行与小游戏构建 | 引擎本体 |
| 热更新 | HybridCLR | 运行时加载 `HotUpdate.dll` | UPM 包；生成物在 `Assets/HybridCLRGenerate/` |
| 资源系统 | YooAsset 2.3.19 | Bundle、清单、下载、缓存和异步资源加载 | UPM 包；小游戏文件系统在 `Assets/ToolPackage/YooAsset` |
| 微信平台 | `com.qq.weixin.minigame` + `Assets/WX-WASM-SDK-V2` | 小游戏转换与运行时 API | UPM 转换工具 + Assets 内运行时 SDK |
| 抖音平台 | StarkSDK 6.7.6 | 抖音小游戏构建与运行时 API | `LocalPackages/com.bytedance.starksdk@6.7.6` 本地包 |
| 异步 | UniTask 2.5.10 | 延迟和重复调用 | `Assets/ToolPackage/UniTask` 本地源码 |
| UI | UGUI + TextMeshPro | 页面和文本 | TextMeshPro 在 `Assets/ToolPackage/TextMesh Pro` |
| 动画 | DOTween | UI 补间 | `Assets/ToolPackage/DOTween` 本地源码 |
| 配置 | ExcelDataReader + 自定义生成器 | Excel 转 bytes 与生成代码 | `Assets/Plugins/ExcelDataReader.dll` 预编译库 |
| JSON | Newtonsoft.Json | 云存档序列化等 | NuGetForUnity；亦为 AOT 元数据 DLL 之一 |
| 其他 | Spine、UIParticle、UOS CDN | 动画、UI 粒子和 CDN | UPM |
| UOS 服务 | UOS Launcher / CloudSave / Func Stateless | 云存档与云函数 | UPM；另有 `Assets/UOSLauncherEncrypt`（Launcher 自带加密模块，勿改） |

## 2. 程序集架构

### 2.1 `Invariable`

位置：`Assets/Scripts/Invariable/Invariable.asmdef`

职责：

- 首场景入口与资源更新状态机；
- YooAsset、HybridCLR 和平台 SDK 初始化；
- 游戏全局事件、计时器、音频、UI 注册表；
- UI 基础控件和通用工具；
- 微信/抖音统一接口（含平台登录）；
- 云存档读写入口（`SetCloudData` / `GetCloudData`）；
- 云存档初始化与排行榜拉取（`CloudManager`）；
- 配置表运行时底座（`ConfigManagerCore` / `ConfigReader` / `BinReader` / `DictionaryForConfig`，首包不随导表变化）。

特点：

- `autoReferenced: false`；
- 运行于热更新 DLL 加载之前；
- 修改后不能只替换 `HotUpdate.dll`，通常需要重新构建并发布小游戏基础包；
- 不直接引用 `HotUpdate`，通过字符串和反射启动热更新层；
- 引用 `CloudService`，通过 Func Stateless 远程代理调用云函数。

主要命名空间：`Invariable`。

### 2.2 `HotUpdate`

位置：`Assets/Scripts/HotUpdate/HotUpdate.asmdef`

职责：

- 业务入口；
- 业务 UI；
- 可更新的玩法逻辑；
- Excel 生成配置。

特点：

- 被 HybridCLR 标记为热更新程序集；
- 可以引用 `Invariable` 的公共能力；
- 发布时作为加密 `.dll.bin` 放入 YooAsset 资源；
- 当前入口必须保持为 `HotUpdate.StartGame.Play()`，除非同时修改反射入口。

主要命名空间：`HotUpdate`。

### 2.3 `CloudService`

位置：`Assets/Scripts/CloudService/`

职责：

- UOS Func Stateless 云函数（平台登录换取云存档令牌、排行榜快照读写等）；
- 客户端经 SDK 远程代理调用，不在客户端执行函数体中的密钥逻辑；
- 云存档相关数据模型（一类一文件，放在 `CloudService/Model/`）。

特点：

- 独立程序集，被 `Invariable` 引用；
- `autoReferenced: false`；
- 修改后需重新构建并发布小游戏基础包，不能只热更；
- 所有游戏可共享同一 UOS App；每个游戏工程在 `CloudHelper.Secrets` 中只配置本游戏密钥；
- 云函数类与 `Model/` 数据类同属 `Assets/Scripts/CloudService/` 目录树，满足 Func Stateless 同目录打包约束。

主要命名空间：`CloudService`。

### 2.4 `MyTools`

位置：`Assets/Editor/MyTools/MyTools.asmdef`

职责：

- Excel 导出；
- HybridCLR DLL 生成与复制；
- YooAsset Bundle 构建；
- 微信/抖音小游戏构建；
- 图集和 `.bin` 导入。

仅包含 Editor 平台，不进入运行时。

### 2.5 `YooAsset.MiniGame`

位置：`Assets/ToolPackage/YooAsset/YooAsset.MiniGame.asmdef`

职责：

- 微信小游戏 YooAsset 文件系统；
- 抖音小游戏 YooAsset 文件系统；
- 对接各平台缓存、文件读取、下载和 AssetBundle 加载 API。

## 3. 启动场景结构

构建场景列表当前只有：

```text
Assets/Scenes/Start.scene
```

场景提供：

- `UI_Root`；
- `Canvas_0` 到 `Canvas_3`；
- 每层的 `UI_Camera`；
- 每层的 `Ts_Panel` 页面挂载点；
- `EventSystem`；
- `Launcher` 启动组件。

UI 约定：

```text
UI_Root/
├─ Canvas_0/
│  ├─ UI_Camera
│  └─ Ts_Panel
├─ Canvas_1/
│  ├─ UI_Camera
│  └─ Ts_Panel
├─ Canvas_2/
│  ├─ UI_Camera
│  └─ Ts_Panel
└─ Canvas_3/
   ├─ UI_Camera
   └─ Ts_Panel
```

代码通过 `Utils.UICamera` / `Utils.UIRoot` 等属性访问这些固定路径，重命名节点时必须同步检查：

- `Launcher.cs`
- `Utils.cs`
- `HotUpdateUtils.cs`
- `UIDrag.cs`
- `Rocker.cs`

约定层级：

| layer 参数 | 典型用途 | 当前示例 |
|---:|---|---|
| 0 | 主界面/普通页面 | `MainPanel` |
| 1 | 较高普通界面 | 暂无明确示例 |
| 2 | 对话框/弹窗 | `TipsPanel` |
| 3 | 顶层提示 | `FloatTextPanel` |

## 4. 完整启动流程

### 4.1 `Launcher.Awake`

文件：`Assets/Scripts/Invariable/Workflow/Launcher.cs`

行为：

1. 编辑器使用 `EPlayMode.EditorSimulateMode`；
2. 非编辑器使用 `EPlayMode.WebPlayMode`；
3. 创建常驻 `GameManager`；
4. 创建常驻 `AudioManager`，并附加 `AudioListener`。

Manager 创建依赖：

```csharp
Utils.CreateManagerInstance("GameManager");
Utils.CreateManagerInstance("AudioManager", new string[] { "AudioListener" });
```

因此 Manager 类名、GameObject 名和反射查找名存在强约定。

### 4.2 `Launcher.OnEnable`

注册三个字符串事件：

| 事件名 | 参数 | 监听用途 |
|---|---|---|
| `Launcher_ShowTips` | `string` | 显示加载描述 |
| `Launcher_ShowProgress` | `DownloadProgressInfo`（`CurrentBytes` / `TotalBytes`） | 显示下载进度 |
| `Launcher_StartGame` | `object`（传 `null`） | 销毁加载面板和 `Launcher`；由 HotUpdate 层 `MainPanel.Awake` 触发（Invariable 层只订阅不发布） |

### 4.3 `Launcher.Start`

创建状态机并注册：

```text
InitializeYooAsset
CheckCatalogUpdate
CheckResourceUpdates
HotUpdateOver
```

同时：

- 将 `EPlayMode` 写入状态机黑板；
- 对 `UI_Root` 调用 `DontDestroyOnLoad`；
- 查找本地加载面板；
- 若不存在，从 `Resources/LocalAssets/HotUpdatePanel` 实例化；
- 从 `InitializeYooAsset` 开始执行。

### 4.4 `InitializeYooAsset`

文件：`Assets/Scripts/Invariable/Workflow/InitializeYooAsset.cs`  
远程服务实现：`Assets/Scripts/Invariable/Workflow/RemoteServices.cs`

职责：

1. 若 `YooAssets.Initialized` 已为 true，直接跳到 `HotUpdateOver`（跳过清单检查与资源下载）；
2. 否则直接取 `InvariableConst.CDNPath` 作为 CDN 根地址；
3. 初始化 YooAsset，并设置 `YooAssets.SetOperationSystemMaxTimeSlice(1000)`；
4. 创建或获取包 `MyPackage`；
5. 设置为默认包；
6. 按模式初始化文件系统。

编辑器：

```text
EditorSimulateModeHelper.SimulateBuild("MyPackage")
```

小游戏：

```text
CDN 根地址 = InvariableConst.CDNPath + "/yoo"
defaultHostServer = fallbackHostServer（主备同址，备线未单独配置）
WebPlayModeParameters
  -> SdkManager.InitializeYooAsset
  -> RemoteServices
  -> 微信或抖音自定义文件系统
```

失败时仅提示「资源加载失败，请检查网络后重启游戏」，无重试，状态机停在本节点。

注意：编辑器域重载或二次进入启动流程时，若 YooAsset 已初始化，会走短路进入 `HotUpdateOver`，并再次执行清缓存、`InitSDK`、`InitCloudData` 全套流程；排查「未重新拉清单/未下载」时需先确认此分支。

### 4.5 `CheckCatalogUpdate`

按顺序执行：

1. `RequestPackageVersionAsync(false)` 获取远程包版本；
2. `UpdatePackageManifestAsync(packageVersion)` 更新清单；
3. 成功后进入 `CheckResourceUpdates`。
4. 失败后则无法进入下一步。

### 4.6 `CheckResourceUpdates`

创建下载器：

```csharp
Package.CreateResourceDownloader(
    downloadingMaxNum: 10,
    failedTryAgain: 3
);
```

- 无文件需下载：直接进入 `HotUpdateOver`；
- 有文件：注册下载回调并开始下载；
- 进度通过 `Launcher_ShowProgress` 传递；
- 下载成功进入 `HotUpdateOver`；
- 失败后则无法进入下一步。

### 4.7 `HotUpdateOver`

按顺序执行：

1. 清理未使用清单缓存；
2. 清理未使用 Bundle 缓存；
3. 初始化平台 SDK；
4. 初始化云存档（平台登录 → 云函数换取云存档令牌 → 拉取云端存档）；
5. 加载 AOT 补充元数据；
6. 加载 `HotUpdate.dll`；
7. 反射调用热更新入口。

反射契约：

```csharp
Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
MethodInfo method = type.GetMethod("Play", ...);
method.Invoke(null, null);
```

因此以下任一变化都必须同步修改 `HotUpdateOver.cs`：

- 命名空间；
- 类名；
- 方法名；
- 方法是否为静态；
- 方法参数列表。

### 4.8 `HotUpdate.StartGame.Play`

当前行为：

```csharp
Utils.OpenUIPrefabPanel("MainPanel", 0);
```

进入前由 `HotUpdateOver` 提示“即将进入游戏...”。`MainPanel.Awake` 触发 `Launcher_StartGame`，销毁加载面板和 Launcher。

## 5. HybridCLR DLL 加载

文件：`Assets/Scripts/Invariable/Manager/YooAssetManager.cs`

### 5.1 编辑器

不加载 DLL 二进制，直接查找已编译程序集：

```csharp
AppDomain.CurrentDomain.GetAssemblies()
    .First(a => a.GetName().Name == "HotUpdate");
```

这意味着编辑器测试成功并不能证明 `.dll.bin`、AOT 元数据或远程 Bundle 正确。

### 5.2 真机小游戏

运行时硬编码资源平台前缀为 `MiniGame`，AOT 程序集列表单一事实源为 `InvariableConst.AotDllNames`（与编辑器 `DllTool` 复制元数据 DLL 共用），并行加载：

```text
MiniGame_mscorlib.dll
MiniGame_System.dll
MiniGame_System.Core.dll
MiniGame_Newtonsoft.Json.dll
```

对每个 DLL 调用：

```csharp
RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.SuperSet);
```

全部完成后加载：

```text
MiniGame_HotUpdate.dll
```

再执行：

```csharp
Assembly.Load(bytes)
```

对应资源源文件：

```text
Assets/GameAssets/DLL/MiniGame/
├─ mscorlib.dll.bin
├─ System.dll.bin
├─ System.Core.dll.bin
├─ Newtonsoft.Json.dll.bin
└─ HotUpdate.dll.bin
```

这些文件不是普通 DLL 原文，而是经 `ConfigUtils.SaveSafeFile` 序列化、GZip 压缩并 AES 加密后的 `.bin`。

任一 AOT / HotUpdate DLL 异步加载失败时，`GameLog.Error` 输出具体 DLL 名；失败路径不再继续加载 `HotUpdate.dll`，启动停在当前阶段。

## 6. YooAsset 资源架构

包名固定为：

```text
MyPackage
```

资源收集配置：

```text
Assets/AssetBundleCollectorSetting.asset
```

收集组：

| 组 | 目录 | 地址规则 | 打包规则 |
|---|---|---|---|
| Animation | `GameAssets/Animation` | Group + FileName | PackGroup |
| Atlas | `GameAssets/Atlas/Atlas01`、`Atlas02`、`Atlas03`（逐目录注册，非通配；新图集目录需手动加收集器） | Group + FileName | PackCollector |
| Audios | `GameAssets/Audios` | Group + FileName | PackGroup |
| Materials | `GameAssets/Materials` | Group + FileName | PackGroup |
| Png | `GameAssets/Png` | Group + FileName | PackGroup |
| Prefabs | `GameAssets/Prefabs/UI` | Group + FileName | PackCollector |
| Scenes | `GameAssets/Scenes` | Group + FileName | PackGroup |
| Config | `GameAssets/Config` | Group + FileName | PackGroup |
| DLL | `GameAssets/DLL` | Folder + FileName | PackGroup |

当前代码中的地址示例：

| 资源 | 地址 |
|---|---|
| 主页面 Prefab | `Prefabs_MainPanel` |
| 提示弹窗 | `Prefabs_TipsPanel` |
| 飘字面板 | `Prefabs_FloatTextPanel` |
| BGM | `Audios_bgm` |
| 灰度材质 | `Materials_GrayscaleMaterial` |
| 遮罩灰度材质 | `Materials_UIMaskGrayscaleMaterial` |
| 独立图片 | `Png_{文件名}` |
| 图集 | `Atlas_{图集名}` |
| 配置表 bytes | `Config_{表名}`（如 `Config_Player`） |
| 热更新 DLL | `MiniGame_HotUpdate.dll` |

图集构建工具：

- 位置：`Assets/Editor/MyTools/AtlasBuilder/`（`AtlasBuilder.cs` + `AtlasBuilder.asset`）；
- 用途：**TMP 表情包图集**专用（TextMesh Pro Sprite Asset 流水线），不是 YooAsset UI 图集构建器；
- 无顶部菜单；在 `AtlasBuilder.asset` 的 Inspector 中配置 `m_atlasName`、`m_directorys` 后，通过 ContextMenu `BuildAtlas` 触发；
- 输出到 `Assets/Editor/MyTools/AtlasBuilder/{图集名}/`（Editor 目录）；按 TMP 表情包工作流接入，与 `GameAssets/Atlas` 的 YooAsset 收集路径相互独立。

### 6.1 资源句柄

`YooAssetManager` 缓存：

- `Dictionary<string, AssetHandle> m_assetHandles`
- `Dictionary<string, List<Action<object>>> m_pendingCallbacks`（同地址在途去重）
- `Dictionary<string, float> m_lastAccessTimes`（最近访问时间，供闲置逐出）
- `Dictionary<string, SceneHandle> m_sceneHandles`

`AsyncLoadAsset<T>`：

- 已缓存时刷新访问时间并直接回调 `AssetObject`；
- 同地址加载中时追加回调，完成后一并通知；
- 首次异步加载并缓存句柄，成功后刷新访问时间；
- 失败写 `GameLog.Error`，并对全部等待回调传入 `null`。

闲置逐出（与配置表 `ConfigFormat` 节奏对齐）：

- 闲置阈值 `180s`，清扫周期 `30s`（计时器 key `InvariableConst.Timer_YooAsset_TickEvict`）；
- 白名单前缀不释放：`Audios_`、`Config_`、`MiniGame_`（音频/配置秒回/程序集无释放价值）；
- 其余地址闲置超时后调用 `ReleaseAsset`；已实例化的 Prefab 实例不受句柄释放影响（YooAsset 引用计数）。

资源释放：

- `ReleaseAsset(address)`：按地址释放句柄，并清理对应图集 Sprite 缓存；
- `UnLoadAsset()`：释放当前缓存中的全部普通资源，并清空 Sprite 缓存；
- `UnloadUnusedAssets(callBack)`：调用 YooAsset 卸载未使用资源；
- `UnLoadScene(address)`：仅释放对应场景句柄，不连带释放普通资源。

## 7. 全局管理器

### 7.1 MonoBehaviour Manager

| Manager | 创建方式 | 用途 |
|---|---|---|
| `GameManager` | `Launcher.Awake` 创建常驻对象 | 事件、延迟计时（UniTask）、循环计时（秒/帧最小堆）；`OnApplicationPause` / `OnApplicationQuit` / `OnDestroy` 调用 `CloudManager.FlushCloudData` |
| `AudioManager` | `Launcher.Awake` 创建常驻对象 | BGM/SFX 通道、音量/静音（本地持久化）、加载排队、同名 SFX 打断重播 |

二者使用私有静态字段 `m_instance`，在 `Awake` 赋值；对外暴露 `Instance`（为空时打 Error）与 `HasInstance`（判空不打日志）。

### 7.2 普通 C# Singleton

| Manager | 用途 |
|---|---|
| `YooAssetManager` | 包、资源、场景、DLL |
| `UIManager` | 已打开页面字典；提供 `CloseUIPanel` / `CloseAllUIPanel`；`TipsPanel` 关闭时隐藏复用（`FloatTextPanel` 自管理复用） |
| `SdkManager` | 平台 SDK、平台登录、本地/云读写入口、键盘、广告、分享、适配 |
| `CloudManager` | 云存档初始化、云缓存、排行榜快照上报（ReportRankScore）与拉取（GetAllCloudData 读 Top100 快照）、云函数代理；写后防抖上传（2s），`FlushCloudData` 立即上传脏数据 |

基类：

```csharp
Singleton<T> where T : new()
```

它们不是 Unity 组件，没有 `Update`、`OnDestroy` 等生命周期。

## 8. 事件系统

实现：`GameManager`

数据结构：

```csharp
Dictionary<string, List<Delegate>>
```

API（仅泛型）：

```csharp
AddEventListener<T>(key, callback);
RemoveEventListener<T>(key, callback);
InvokeEventCallBack<T>(key, arg);
```

无参通知：

```csharp
InvokeEventCallBack<object>(key, null);
```

使用规则：

1. 事件/延迟调用 key 必须使用常量：跨层契约进 `InvariableConst`，HotUpdate 业务进 `HotUpdateConst`（均用 `#region` 分区）；
2. 注册通常放 `OnEnable`，移除放 `OnDisable`；
3. 监听与触发的泛型参数类型必须一致；
4. `InvokeEventCallBack` 使用快照遍历 + 单回调异常隔离，回调内增删监听或抛异常不会打断其他监听者。

## 9. 计时与重复调用

实现：`GameManager` + UniTask。

API：

```csharp
DelayCallFrames(key, callback, frame);
DelayCallSeconds(key, callback, time);
RepeatingCallFrames(key, callback, frame = 1, immediately = true);
RepeatingCallSeconds(key, callback, time = 1f, immediately = true);
CancelInvokeByKey(key);
```

约束：

- `key` 全局唯一；
- 延迟键与循环键任一已存在时，新调用直接返回；
- 调用方销毁或禁用时应主动取消；
- 一次性延迟完成后会移除对应 key；
- 循环计时由 `Update` 驱动最小堆；`immediately` 为 true 时注册后立即执行一次；
- `DelayCallSeconds` 与 `RepeatingCallSeconds` 均受 `Time.timeScale` 影响；
- `CancelInvokeByKey` 仅当 key 存在时输出 `GameLog.Info(key + "取消调用")`，key 不存在直接返回。

现有使用示例：`FloatTextPanel`。

## 10. UI 框架

### 10.1 页面打开

打开页面应使用：

```csharp
Utils.OpenUIPrefabPanel(prefabName, layer, callback);
```

Tips/FloatText 业务封装走 `HotUpdateUtils.OpenTipsPanel` / `ShowFloatText`（内部仍调用 `Utils.OpenUIPrefabPanel`）。

执行过程：

1. 根据文件名得到页面名；
2. 若 `UIManager.AllPanel` 已有该页面则重新激活并回调；
3. 若同名页面正在加载中则忽略本次打开；
4. 加载地址 `Prefabs_{页面名}`；
5. 实例化到 `UI_Root/Canvas_{layer}/Ts_Panel`；
6. 通过类型名查找或动态添加组件（解析顺序覆盖 `Invariable` / `HotUpdate` / 热更程序集）；
7. 将其作为 `UIPanel` 注册；
8. 调用回调。

### 10.2 页面关闭

所有页面基类：

```csharp
public class UIPanel : MonoBehaviour
```

调用 `Close()`：

- 带 `UIPopup`：先播放关闭动画，再走关闭流程；
- `TipsPanel`（`UIManager` 池化名单）：隐藏复用，不从字典移除；
- 其余普通页面（含 `FloatTextPanel`）：从 UIManager 移除并 `Destroy`。

### 10.3 当前页面

| 页面 | 脚本 | 层级 | 用途 |
|---|---|---:|---|
| MainPanel | `HotUpdate.UI/MainPanel/MainPanel.cs` | 0 | 当前主界面 |
| TipsPanel | `HotUpdate.UI/Popup/TipsPanel.cs` | 2 | 单/双按钮提示 |
| FloatTextPanel | `HotUpdate.UI/Popup/FloatTextPanel.cs` | 3 | 可复用飘字提示 |

辅助接口：

```csharp
HotUpdateUtils.OpenTipsPanel(...);
HotUpdateUtils.ShowFloatText(...);
```

### 10.4 UI 基础组件

| 组件 | 用途 |
|---|---|
| `UIButton` | 单击、双击、按下、抬起、长按、缩放反馈；每类监听覆盖赋值（非追加）；双击判定窗口 0.15s；长按阈值 0.2s；注册双击后单击会延迟 0.15s 且可能被双击吞掉 |
| `UIPanel` | 页面基类 |
| `UIPopup` | 弹窗开关动画；入场动画在 `OnEnable`（每次激活重播并重置缩放）；`m_tsTrans` 所在物体须同时挂 `CanvasGroup`；DOTween 使用 `SetTarget` / `DOKill` |
| `LoopScrollList` | 横向/纵向循环列表；列表项缓存索引 |
| `MiniInputField` | 调起小游戏原生键盘 |
| `ScreenAdapter` | `[ExecuteInEditMode]`；注册安全区适配；实际偏移由 `SdkManager.GetSafeAnchor` 写死为 Left/Bottom=30/130、Right/Top=30/90，非设备 SafeArea；编辑期 `OnEnable` 即改节点偏移 |
| `BgAdapter` | `[ExecuteInEditMode]`；背景等比铺满；编辑期同样生效 |
| `UIDrag` | UI 拖拽及 ScrollRect 事件转发；拖拽回调阶段 1=开始/2=拖拽中/3=结束 |
| `Rocker` | 虚拟摇杆（`SetMoveFunc(Action<Vector2>)` 输出方向归一化 × 力度 0~1，手柄跟随并松开回中） |
| `CircleImage` | 圆形 Sprite UI 网格 |
| `CircleRawImage` | 圆形 RawImage UI 网格 |
| `PolygonImage` | 基于 PolygonCollider2D 的非矩形射线检测 |

## 11. 音频

接口：

```csharp
AudioManager.Instance.PlayBGM("bgm");
AudioManager.Instance.PlaySFX("click");
AudioManager.Instance.PauseAudio("bgm");
AudioManager.Instance.StopAudio("bgm");
AudioManager.Instance.PauseAudio(); // 空名或省略参数：暂停全部
AudioManager.Instance.StopAudio();  // 空名或省略参数：停止全部
AudioManager.Instance.SetMasterVolume(1f);
AudioManager.Instance.SetBGMVolume(1f);
AudioManager.Instance.SetSFXVolume(1f);
AudioManager.Instance.SetMute(false); // 音量设置经 SdkManager 本地存储持久化
```

资源地址固定拼接为：

```text
Audios_{name}
```

BGM 为单通道循环（子物体 `BGM`）；SFX 按名各自维护独立 `AudioSource`，同名打断重播；音量/静音经 `SdkManager` 本地存储持久化；BGM 加载中切歌会排队到加载完成后播放。

## 12. 配置系统

配置源：

```text
Excel/*（.xlsx/.xls）
```

`.xlsx/.xls` 只读第一个 sheet。

目录：

```text
Assets/Scripts/Invariable/Config/          # 运行时底座（首包）
Assets/Scripts/HotUpdate/Config/           # ConfigManager 转发层
Assets/Scripts/HotUpdate/Config/Generated/ # Config_*.cs + ConfigManager.Preload.cs
```

菜单：

```text
VastStarryRiver/Config/导出Excel配置
VastStarryRiver/Config/校验配置数据
```

「校验配置数据」回读 `GameAssets/Config/*.bytes` 与 Excel 源表逐字段比对（float 容差 `1e-4`），结果以 `[OK]` / `[ERROR]` / `[MISSING]` 输出到 Console；不依赖 HotUpdate 程序集。

导表产物：

- `Assets/GameAssets/Config/{Table}.bytes`（YooAsset Config 组，可热更；单文件含 magic(CFGT)+schemaHash+count+ids+rowSize+数据区+字符串区；运行时以 `TextAsset` 加载，地址 `Config_{表名}`；加载时校验 schemaHash，不匹配即报错需重新导表）
- `Assets/Scripts/HotUpdate/Config/Generated/Config_{Table}.cs`（行类型 + `ConfigManager` partial API，含 `SchemaHash`）
- `Assets/Scripts/HotUpdate/Config/Generated/ConfigManager.Preload.cs`（`PreloadAll` / `ClearAll`）

运行时 API（回调式）。每表由生成代码提供 `Get{表}ByID` / `Get{表}ByIDs` / `Get{表}` / `GetAll{表}` / `Clear{表}`；`PreloadAll` / `ClearAll` 由 `ConfigManager.Preload.cs` 提供：

```csharp
ConfigManager.GetPlayerByID(1, row => { ... }); // Config_Player
ConfigManager.GetPlayerByIDs(new[] { 1, 2 }, list => { ... });
ConfigManager.GetRoleRune(dic => { ... });
ConfigManager.GetAllRoleRune(
    list => { ... },                    // 完成才交付完整列表
    (loaded, total) => { ... });        // 可选进度；>500 行分帧物化
ConfigManager.ClearRoleRune();
ConfigManager.PreloadAll(() => { ... });
ConfigManager.ClearAll();
```

缓存与逐出：闲置超过 180s 的表会逐出解析层（Reader/行对象/字符串缓存），YooAsset handle 与 bytes 常驻，再次访问同帧秒回。不要跨帧长期缓存 `DictionaryForConfig`；逐出后继续用会报错，需重新调用 `ConfigManager.Get{表}`。

### 12.1 Excel 源表格式

表头固定 3 行，第 4 行起为数据（全表至少 4 行）：

| 行 | 含义 |
|---:|---|
| 1 | 字段名；首列强制视为 `Id` |
| 2 | 类型：`int` / `float` / `string`（不区分大小写） |
| 3 | 注释（可空） |
| 4+ | 数据行 |

约束：

- 文件名须为合法 C# 标识符；
- `Id` 必须是标量 `int`，不能是数组；
- 字段按 baseName 字典序、同前缀按数字后缀排序后写入 bytes 与生成代码，`Id` 强制首位；布局顺序不等于 Excel 列序；
- 定长数组：列名使用 `字段名+序号`（如 `Reward1`/`Reward2`），同前缀连续列且类型一致，生成 `字段名` 数组；
- 空单元格：`int`/`float` 按 0，`string` 按空串。

支持类型：

```text
int, float, string,
定长 int[] / float[] / string[]
```

标量示例：

| Id | StartLv | Speed | Name |
|---|---|---|---|
| int | int | float | string |
| 编号 | 开启等级 | 速度 | 名称 |
| 1 | 10 | 3.5 | 新手 |

定长数组示例：

| Id | Reward1 | Reward2 | Pos1 | Pos2 | Tag1 | Tag2 |
|---|---|---|---|---|---|---|
| int | int | int | float | float | string | string |
| 编号 | 奖励1 | 奖励2 | X | Y | 标签1 | 标签2 |
| 1 | 100 | 200 | 1.5 | 2.5 | 近战 | 物理 |

生成字段：`int[] Reward`、`float[] Pos`、`string[] Tag`。

### 12.2 设计要点

- Excel 为唯一事实源；
- 底座在 `Invariable.ConfigManagerCore`，HotUpdate `ConfigManager` 转发；
- 整表 bytes 可驻留，行对象按 ID 惰性反序列化；
- 纯数值改动只需导表 + 构建 AssetBundle；表结构变更才需重导 HybridCLR DLL；改底座需重发基础包；
- 不应手工修改生成文件。

当前生成表：

- `Config_Player`
- `Config_RoleRune`

## 13. CDN 根地址

运行时 CDN 根地址为编译期常量 `InvariableConst.CDNPath`（`Assets/Scripts/Invariable/Utils/InvariableConst.cs`，`#region 游戏资源`）。YooAsset 远程根为 `{CDNPath}/yoo`。

打包微信/抖音小游戏时，菜单会把该常量写入平台配置资产的 `CDN` 字段后再构建：

- 微信：`Assets/WX-WASM-SDK-V2/Editor/MiniGameConfig.asset`、`Assets/Settings/Build Profiles/WeChat Profile.asset`
- 抖音：`Assets/Settings/Build Profiles/DouYin Profile.asset`、`Assets/Editor/StarkBuilderSetting.asset`

只同步 `CDN` 字段；`StreamCDN` / `AssetsUrl` / `wasmResourceUrl` 不改。Profile 里的 CDN 无需手填。

`ConfigUtils.CdnPath` 是工程根下的本地 `CDN/` 暂存目录（复制 bundle / unityweb.bin 用），与远程根 `InvariableConst.CDNPath` 不是同一概念。

`InvariableConst.CDNPath` 属 `Invariable`，修改后必须重新构建并发布小游戏基础包，不能只热更。

## 14. 工具类

`Utils` 按 `#region` 分区（共 9 个）：UI 查找与对象操作 / 面板开关 / 文本与灰态 / Sprite 缓存 / 按钮监听 / 动画 / 面板加载 / 组件与类型解析 / 管理器与杂项。

主要能力：

- 固定 UI Camera、UI Root 查找（`Utils.UICamera` / `Utils.UIRoot`，路径常量来自 `InvariableConst`）；
- GameObject/Transform 获取和克隆；
- 显隐（`SetActive` 默认 `isActive = false`，省略参数即隐藏）、`HideAllChildren`；
- 文本、图片、灰度材质；
- UIButton 事件包装；
- Animation 播放；
- 按字符串查找/添加组件（`GetComponent` / `AddComponent`，含 `HotUpdate` 程序集类型解析）；
- UI 页面打开/关闭（`OpenUIPrefabPanel` / `CloseUIPrefabPanel`）；面板父节点按 layer 选择 `InvariableConst.UIPanelPath_0..3`；
- 图集 Sprite 缓存与清理（`ClearSpriteCache`）；
- Manager GameObject 创建；
- 文件大小格式化（`FormatFileByteSize`，与 `ConfigUtils.FormatFileByteSize` 重复实现）；
- HTML 颜色解析。

这类 API 依赖字符串路径和资源地址，使用前应优先检查空对象与地址有效性。

## 15. 日志输出

- `Invariable` / `HotUpdate` / `MyTools` 业务日志使用 `GameLog.Info` / `GameLog.Error`，禁止直接调用 `UnityEngine.Debug.Log` / `LogWarning` / `LogError`；
- `GameLog.Info` 仅编辑器环境输出（包内剔除）；`GameLog.Error` 始终输出；
- 映射：`Debug.Log` / `Debug.LogWarning` → `GameLog.Info`；`Debug.LogError` → `GameLog.Error`；
- `CloudService` 云函数体仍使用 `UnityEngine.Debug`（云函数约束，且避免与 `Invariable` 循环引用）；
- 例外：`GameLog.cs` 自身实现；第三方库 `ToolPackage` 不在此约束。

## 16. UOS 服务与云存档

`Assets/Resources/UOSSettings.asset` 经 UOS Launcher 关联 UOS App（所有游戏共享同一 App）。

云存档链路：

1. `CloudManager.InitCloudData`；
2. `SdkManager.PlatformLogin`（`WX.Login` / `TT.Login`，抖音 `forceLogin=true`）；
3. Func Stateless 云函数（code 换 openid，再签发云存档令牌）；
4. `AuthTokenManager.SaveToken`（只存 AccessToken + UserId）；
5. CloudSave 单存档 KV 拉取/上传。上传前校验令牌有效性，临期/过期自动重签；遇 401 再重签并重试一次。

数据隔离规则：

- `userID = wx-` / `dy-` + openid（openid 按平台小游戏隔离）；
- 玩家存档 `namespace = kv_{游戏标识}_player`（`CloudManager.CloudSaveGameId`，每个游戏项目必须唯一）；
- 排行榜快照 `namespace = kv_{游戏标识}_rank`，单存档，userId 固定为 `sys`；
- 同游戏同账号才同数据。

客户端入口：`CloudManager` 负责 `InitCloudData` / `GetAllCloudData` / `ReportRankScore`；`SdkManager` 负责 `SetCloudData` / `GetCloudData`（与本地存储同属数据存储模块）。`GetAllCloudData(rankKey, callBack)`：客户端传入排名字段名（如 `"Score"`），云函数校验 `gameId` 与 `CloudHelper.Secrets.GameId` 一致后自行拼 `kv_{gameId}_rank`，只读 Top100 快照（list + 详情 + 下载共 3 次请求），按 `rankKey` 数值降序截取前 `CloudGetAllMaxCount = 100` 名返回。`ReportRankScore(rankKey, score)`：刷新个人纪录时上报，云函数增量维护快照；客户端按 `LocalKey_RankReportedPrefix` 节流，云函数有响应即写本地标记。客户端 SDK 只能读当前玩家自己的存档，也不能直接指定 namespace。业务侧回调类型为 `CloudService.PlayerCloudData`（位于 `Assets/Scripts/CloudService/Model/PlayerCloudData.cs`）。

编辑器环境 `SdkManager.SetCloudData` / `GetCloudData` 走本地存储；真机转发 `CloudManager` 云缓存；云初始化失败后 Set 静默丢弃、Get 返回默认值。

云函数部署：

1. 在 `CloudHelper.Secrets` 填入当前游戏的 `GameId` 与平台 AppID/AppSecret；`CloudManager.CloudSaveGameId` 必须与其一致；
2. 菜单 `UOS -> Func Stateless -> Open Panel` 上传；
3. 切换为远程调用模式。

微信/抖音 MP 后台白名单：`https://a.unity.cn`、`https://a.unity3dcloud.cn`、`https://a2.unity3dcloud.cn`、`https://a3.unity3dcloud.cn`（CDN 资源）；`https://save.unity.cn`、`https://uos-save-bluecloud-1301389817.cos.ap-shanghai.myqcloud.com`、`https://stateless.unity.cn`、`https://p.unity.cn`。