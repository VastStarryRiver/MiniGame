# 框架架构与运行流程

## 1. 技术栈

| 类别 | 技术/版本 | 用途 |
|---|---|---|
| 引擎 | 团结引擎 1.6.8 / Unity 2022.3.61t9 | 游戏运行与小游戏构建 |
| 热更新 | HybridCLR | 运行时加载 `HotUpdate.dll` |
| 资源系统 | YooAsset 2.3.19 | Bundle、清单、下载、缓存和异步资源加载 |
| 微信平台 | `com.qq.weixin.minigame` | 微信小游戏转换与运行时 API |
| 抖音平台 | StarkSDK 6.7.6 | 抖音小游戏构建与运行时 API |
| 异步 | UniTask 2.5.10 | 延迟和重复调用 |
| UI | UGUI + TextMeshPro | 页面和文本 |
| 动画 | DOTween | UI 补间 |
| 配置 | ExcelDataReader + 自定义生成器 | Excel 转嵌入式 C# 配置 |
| 其他 | Spine、UIParticle、UOS CDN | 动画、UI 粒子和 CDN |

## 2. 程序集架构

### 2.1 `Invariable`

位置：`Assets/Scripts/Invariable/Invariable.asmdef`

职责：

- 首场景入口与资源更新状态机；
- YooAsset、HybridCLR 和平台 SDK 初始化；
- 游戏全局事件、计时器、音频、UI 注册表；
- UI 基础控件和通用工具；
- 微信/抖音统一接口。

特点：

- `autoReferenced: false`；
- 运行于热更新 DLL 加载之前；
- 修改后不能只替换 `HotUpdate.dll`，通常需要重新构建并发布小游戏基础包；
- 不直接引用 `HotUpdate`，通过字符串和反射启动热更新层。

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

### 2.3 `MyTools`

位置：`Assets/Editor/MyTools/MyTools.asmdef`

职责：

- Excel/WebData 导出；
- HybridCLR DLL 生成与复制；
- YooAsset Bundle 构建；
- 微信/抖音小游戏构建；
- 图集和 `.bin` 导入。

仅包含 Editor 平台，不进入运行时。

### 2.4 `YooAsset.MiniGame`

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

代码大量通过 `GameObject.Find` 使用这些固定路径，重命名节点时必须同步检查：

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
Utils.CreateManagerInstance("AudioManager", new[] { "AudioListener" });
```

因此 Manager 类名、GameObject 名和反射查找名存在强约定。

### 4.2 `Launcher.OnEnable`

注册三个字符串事件：

| 事件名 | 参数 | 监听用途 |
|---|---|---|
| `Launcher_ShowTips` | `string` | 更新加载描述 |
| `Launcher_ShowProgress` | `List<long>`，元素为当前/总字节数 | 更新下载进度 |
| `Launcher_StartGame` | 无 | 销毁加载面板和 `Launcher` |

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

职责：

1. 读取 `Resources/LocalAssets/WebData.bin`；
2. 解密并解析 CDN 等 Web 配置；
3. 初始化 YooAsset；
4. 创建或获取包 `MyPackage`；
5. 设置为默认包；
6. 按模式初始化文件系统。

编辑器：

```text
EditorSimulateModeHelper.SimulateBuild("MyPackage")
```

小游戏：

```text
CDN 根地址 = ConfigUtils.CDNPath + "/yoo"
WebPlayModeParameters
  -> SdkManager.InitializeYooAsset
  -> 微信或抖音自定义文件系统
```

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
4. 加载 AOT 补充元数据；
5. 加载 `HotUpdate.dll`；
6. 反射调用热更新入口。

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
HotUpdateUtils.OpenUIPrefabPanel("MainPanel", 0);
```

`MainPanel.Awake` 触发 `Launcher_StartGame`，销毁加载面板和 Launcher。

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

运行时硬编码资源平台前缀为 `MiniGame`，并行加载：

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
| Atlas | `GameAssets/Atlas/*` | Group + FileName | PackCollector |
| Audios | `GameAssets/Audios` | Group + FileName | PackGroup |
| Materials | `GameAssets/Materials` | Group + FileName | PackGroup |
| Png | `GameAssets/Png` | Group + FileName | PackGroup |
| Prefabs | `GameAssets/Prefabs/UI` | Group + FileName | PackCollector |
| Scenes | `GameAssets/Scenes` | Group + FileName | PackGroup |
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
| 热更新 DLL | `MiniGame_HotUpdate.dll` |

### 6.1 资源句柄

`YooAssetManager` 使用两个字典缓存：

- `Dictionary<string, AssetHandle> m_assetHandles`
- `Dictionary<string, SceneHandle> m_sceneHandles`

`AsyncLoadAsset<T>`：

- 首次异步加载并缓存句柄；
- 已缓存时直接回调 `AssetObject`；
- 失败只写错误日志，没有失败回调。

`UnLoadAsset()` 会释放**当前缓存中的全部普通资源**，不是释放单个地址。

`UnLoadScene(address)` 会先调用 `UnLoadAsset()`，再卸载目标场景。因此卸载任意场景会同时释放所有普通资源句柄，新增场景逻辑时必须注意。

## 7. 全局管理器

### 7.1 MonoBehaviour Manager

| Manager | 创建方式 | 用途 |
|---|---|---|
| `GameManager` | `Launcher.Awake` 创建常驻对象 | 事件、协程、UniTask 计时 |
| `AudioManager` | `Launcher.Awake` 创建常驻对象 | 音频加载、播放、暂停和停止 |

二者使用公开静态字段 `Instance`，在 `Awake` 赋值。

### 7.2 普通 C# Singleton

| Manager | 用途 |
|---|---|
| `YooAssetManager` | 包、资源、场景、DLL |
| `UIManager` | 已打开页面字典 |
| `SdkManager` | 平台 SDK、存储、键盘、广告、适配 |

基类：

```csharp
Singleton<T> where T : new()
```

它们不是 Unity 组件，没有 `Update`、`OnDestroy` 等生命周期。

## 8. 事件系统

实现：`GameManager`

数据结构：

```csharp
Dictionary<string, List<Action<object>>>
```

API：

```csharp
AddEventListener(key, callback);
RemoveEventListener(key, callback);
InvokeEventCallBack(key, arg);
```

使用规则：

1. 事件名当前使用字符串，没有编译期校验；
2. 注册通常放 `OnEnable`，移除放 `OnDisable`；
3. 参数由 `object` 传递，监听方必须正确转换；
4. 新事件应在文档或常量类集中登记，避免拼写不一致；
5. 回调中修改同一事件监听列表存在遍历风险，应谨慎。

## 9. 计时与重复调用

实现：`GameManager` + UniTask。

API：

```csharp
DelayCallFrames(key, callback, frame);
DelayCallSeconds(key, callback, time);
RepeatingCallFrames(key, callback, frame);
RepeatingCallSeconds(key, callback, time);
CancelInvokeByKey(key);
```

约束：

- `key` 全局唯一；
- 字典已有相同 key 时，新调用直接返回；
- 调用方销毁或禁用时应主动取消；
- 当前一次性延迟完成后不会自动移除 key，复用前需要取消清理；

现有使用示例：`FloatTextPanel`。

## 10. UI 框架

### 10.1 页面打开

热更新层应使用：

```csharp
HotUpdateUtils.OpenUIPrefabPanel(prefabName, layer, callback);
```

执行过程：

1. 根据文件名得到页面名；
2. 检查 `UIManager.AllPanel`；
3. 加载地址 `Prefabs_{页面名}`；
4. 实例化到 `UI_Root/Canvas_{layer}/Ts_Panel`；
5. 通过类型名查找或动态添加 `HotUpdate.{页面名}`；
6. 将其作为 `UIPanel` 注册；
7. 调用回调。

为什么热更新页面必须优先使用 `HotUpdateUtils`：

- `Invariable.Utils.AddComponent` 优先查找 `Invariable.{类型名}`；
- `HotUpdateUtils.AddComponent` 能查找 `HotUpdate.{类型名}`；
- 新热更新 UI 脚本位于动态程序集，使用错误工具可能找不到类型。

### 10.2 页面关闭

所有页面基类：

```csharp
public class UIPanel : MonoBehaviour
```

调用 `Close()`：

- 普通页面：从 UIManager 移除并 `Destroy`；
- 带 `UIPopup`：先播放关闭动画，再销毁。

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
| `UIButton` | 单击、双击、按下、抬起、长按、缩放反馈 |
| `UIPanel` | 页面基类 |
| `UIPopup` | 弹窗开关动画 |
| `LoopScrollList` | 横向/纵向循环列表 |
| `MiniInputField` | 调起小游戏原生键盘 |
| `ScreenAdapter` | 注册安全区适配 |
| `BgAdapter` | 背景等比铺满 |
| `UIDrag` | UI 拖拽及 ScrollRect 事件转发 |
| `Rocker` | 虚拟摇杆 |
| `CircleImage` | 圆形 Sprite UI 网格 |
| `CircleRawImage` | 圆形 RawImage UI 网格 |
| `PolygonImage` | 基于 PolygonCollider2D 的非矩形射线检测 |

## 11. 音频

接口：

```csharp
AudioManager.Instance.PlayAudio("bgm", true);
AudioManager.Instance.PauseAudio("bgm");
AudioManager.Instance.StopAudio("bgm");
```

资源地址固定拼接为：

```text
Audios_{name}
```

每个音频名对应一个 `AudioSource` 子对象并长期保存在字典。当前没有音量分组、淡入淡出、销毁或句柄级释放。

## 12. 配置系统

配置源：

```text
Excel/*.xls 或 *.xlsx
```

生成目录：

```text
Assets/Scripts/HotUpdate/Config/Tab_*.cs
```

菜单：

```text
VastStarryRiver/Config/导出Excel配置
```

生成器会先删除整个 `HotUpdate/Config` 目录，再重新生成。

生成类结构：

```csharp
public static class Tab_TableName
{
    public class Row { ... }

    public static void Init(Action onComplete = null);
    public static Row GetConfigByIndex(int index);
    public static Row GetConfigByIndex(string index);
    public static List<Row> GetAllConfigs();
    public static int Count { get; }
}
```

数据直接生成到 `BuildConfigs()` 的 C# 代码中，因此：

- 不需要运行时读取配置；
- 配置会进入 `HotUpdate.dll`；
- 修改 Excel 后必须重新导出 DLL 和 YooAsset；
- 不应手工修改 `Tab_*.cs` 中的数值，因为下次导出会覆盖。

支持类型：

```text
int, float, bool, string,
int[], float[], string[]
```

当前生成表：

- `Tab_Player`
- `Tab_RoleRune`

## 13. WebData 与本地二进制

源文件位于项目根目录：

```text
WebData.txt
```

代码支持行定义：

| 行号（从 0 开始） | 含义 |
|---:|---|
| 0 | CDN 根地址，要求（CDN/yoo/所有Bundle） |
| 1 | 服务器地址，格式（IP:Port） `当前未使用业务服务器` |
| 2 | 下载认证用户名 `当前未使用业务服务器` |
| 3 | 下载认证密码 `当前未使用业务服务器` |

当前启动资源更新只直接使用第 0 行。

编辑器菜单：

```text
VastStarryRiver/Config/导出Web配置
```

输出：

```text
Assets/Resources/LocalAssets/WebData.bin
```

`.bin` 由 `BinImporter` 导入为 `BinAsset`，运行时通过 `Resources.Load<BinAsset>` 读取。

## 14. 工具类

`Utils` 包含：

- 固定 UI Camera、UI Root 和主场景 Camera 查找；
- GameObject/Transform 获取和克隆；
- 显隐、文本、图片、灰度材质；
- UIButton 事件包装；
- Animation 播放；
- UI 页面打开/关闭；
- 按字符串添加组件；
- Manager GameObject 创建；
- HTML 颜色解析。

这类 API 依赖字符串路径和资源地址，新增功能时应优先检查空对象与地址有效性。