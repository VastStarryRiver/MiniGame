# MiniGame 框架文档索引

## 1. 阅读顺序

先阅读本文件，再按需求阅读对应文档：

1. [FrameworkAndProcess.md](Doc/FrameworkAndProcess.md) `框架架构与运行流程`
   - 项目技术栈
   - 程序集边界
   - 启动、资源更新、HybridCLR 热更新流程
   - 管理器、事件、计时器、UI、配置、UOS 云存档等模块
2. [ScriptDevGuide.md](Doc/ScriptDevGuide.md) `脚本开发与修改指南`
   - 新脚本应该放在哪里
   - 新增 UI、业务功能、配置表、资源、平台能力的方法
   - 常用 API、命名约定和修改检查清单
3. [HotUpdateBuildAdapt.md](Doc/HotUpdateBuildAdapt.md) `构建热更新与平台适配`
   - 微信/抖音平台差异
   - WebData、HybridCLR DLL、YooAsset、CDN 和小游戏构建顺序
   - 热更新发布边界（含云函数上传与远程调用）

## 2. 项目一句话架构

这是一个基于团结引擎1.6.8的微信/抖音小游戏框架，使用：

- `Invariable` 程序集承载不可热更的启动框架、平台 SDK、资源管理和基础组件；
- `HotUpdate` 程序集承载可通过 HybridCLR 更新的业务代码、UI 脚本和生成配置；
- `CloudService` 程序集承载 UOS Func Stateless 云函数与云存档数据模型；
- YooAsset 管理远程资源和热更新 DLL；
- 微信 SDK / 抖音 StarkSDK 提供平台能力；
- 启动完成后通过反射调用 `HotUpdate.StartGame.Play()` 进入业务层。

核心调用链：

```text
Assets/Scenes/Start.scene
  -> Invariable.Launcher
  -> InitializeYooAsset
  -> CheckCatalogUpdate
  -> CheckResourceUpdates
  -> HotUpdateOver
  -> SdkManager.InitSDK
  -> CloudManager.InitCloudData
  -> YooAssetManager.PreLoadDll
  -> 反射 HotUpdate.StartGame.Play
  -> HotUpdateUtils.OpenUIPrefabPanel("MainPanel", 0)
```

## 3. 最重要的目录边界

```text
Assets/
├─ Scripts/
│  ├─ Invariable/          # 不可热更：启动、平台、资源、基础能力、云存档客户端
│  ├─ HotUpdate/           # 可热更：业务、UI、生成配置
│  └─ CloudService/        # 不可热更：UOS 云函数与云存档数据模型
├─ GameAssets/             # YooAsset 收集的动态资源
│  ├─ DLL/MiniGame/        # 加密后的 HotUpdate/AOT DLL .bin
│  ├─ Prefabs/UI/          # UI 预制体
│  ├─ Audios/              # 音频
│  ├─ Atlas/               # 图集
│  ├─ Materials/           # 材质
│  ├─ Png/                 # 独立图片
│  └─ Scenes/              # 动态场景
├─ Resources/LocalAssets/  # 首包本地资源：加载面板、WebData.bin
├─ Editor/MyTools/         # 编辑器导表、DLL、资源包和平台构建工具
├─ ToolPackage/YooAsset/   # 微信/抖音 YooAsset 自定义文件系统
└─ Settings/Build Profiles/# 微信和抖音 Build Profile

Excel/                    # 配置源文件
```

## 4. 修改位置快速决策

| 需求 | 默认修改位置 | 是否可只发布热更新 |
|---|---|---:|
| 新玩法、数值逻辑、业务状态 | `Assets/Scripts/HotUpdate` | 是 |
| 新 UI 页面脚本 | `Assets/Scripts/HotUpdate/UI` | 是，但预制体也要进入 YooAsset 更新 |
| 修改 Excel 数值 | `Excel`，然后重新导出配置 | 是 |
| UI 基础控件、资源框架、启动链 | `Assets/Scripts/Invariable` | 否，通常需要重新发布小游戏基础包 |
| 微信/抖音 SDK 能力、平台登录、本地/云读写入口 | `Invariable/Manager/SdkManager.cs` | 否 |
| 云存档初始化、全量拉取、云缓存 | `Invariable/Manager/CloudManager.cs` | 否 |
| 云函数/平台密钥 | `Assets/Scripts/CloudService` | 否；改后需重新上传云函数并切远程调用 |
| 云存档数据模型 DTO | `Assets/Scripts/CloudService/Model` | 否；改契约需同步重新上传云函数 |
| YooAsset 小游戏文件系统 | `Assets/ToolPackage/YooAsset` | 否 |
| 编辑器导出/构建工具 | `Assets/Editor/MyTools` | 不属于运行时发布 |
| 修改启动加载面板 | `Assets/Resources/LocalAssets` 与 `Invariable/Workflow` | 通常否 |
| 新动态图片、音频、Prefab、场景 | `Assets/GameAssets` | 可通过资源更新发布 |
| 修改资源地址规则 | `Assets/AssetBundleCollectorSetting.asset` | 高风险，需重新构建并验证全量资源 |

基本原则：

1. **业务需求优先写入 `HotUpdate`。**
2. 只有“热更新 DLL 加载前必须执行”或“直接依赖平台 SDK”的代码才放入 `Invariable`。
3. `Invariable` 不得直接编译引用 `HotUpdate`，当前通过反射跨越程序集边界。
4. `HotUpdate/Config/Tab_*.cs` 是生成文件，数值修改应改 Excel 后重新导出。
5. 不要直接修改 `Assets/GameAssets/DLL/MiniGame/*.dll.bin`；它们由 DLL 工具生成。

## 5. 后续需求建议

为了快速且安全地新增或修改脚本，最好想先清楚需求内容，具体包含：

```text
【目标】
要新增或修复什么？

【验收条件】
玩家执行什么操作后，应看到什么结果？

【影响平台】
编辑器 / 微信 / 抖音 / 全部

【热更新要求】
是否必须仅通过热更新发布？

【涉及资源】
Prefab、图片、音频、场景、Excel 表名及资源地址（如有）

【复现步骤】
BUG 出现前的操作、实际结果、预期结果、日志或截图

【兼容要求】
是否需要兼容已有存档、旧资源清单或旧版本客户端？
```

## 6. 后续修改代码时的流程

1. 读取本目录文档和相关源码；
2. 判断修改应位于 `HotUpdate`、`Invariable` 还是编辑器工具层；
3. 搜索调用方、Prefab 绑定、YooAsset 地址和平台条件编译；
4. 实施最小范围修改；
5. 检查编译边界、空引用、生命周期、事件/计时器清理；
6. 尽可能进行静态检查或引擎编译验证；
7. 汇报改动文件和内容、发布平台类型、仍需在编辑器/真机完成的验证。

## 7. 当前框架状态摘要

- 引擎：团结引擎 `1.6.8`，对应 Unity `2022.3.61t9`。
- 唯一构建场景：`Assets/Scenes/Start.scene`。
- YooAsset 包名：`MyPackage`。
- HybridCLR 热更新程序集：`HotUpdate`。
- 当前热更新入口：`HotUpdate.StartGame.Play()`。
- 当前首个业务页面：`MainPanel`，UI 层级 `0`。
- UI 根节点依赖固定路径：`UI_Root/Canvas_{0..3}/Ts_Panel`。
- 微信/抖音平台通过条件宏切换：
  - `MINIGAME_SUBPLATFORM_WEIXIN`
  - `MINIGAME_SUBPLATFORM_DOUYIN`
- 当前安全区是固定偏移，不是根据设备实时安全区计算。
- UOS：Launcher / CloudSave / Func Stateless；云存档 namespace 为 `minigame_kv_{CloudManager.CLOUD_SAVE_GAME_ID}`，须与 `CloudHelper.Secrets.GameId` 一致。
- 云读写业务入口：`SdkManager.SetCloudData` / `GetCloudData`；云初始化：`CloudManager.InitCloudData`。