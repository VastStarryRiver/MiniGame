# 脚本开发与修改指南

## 1. 修改前先判断发布边界

### 1.1 应放入 `HotUpdate` 的代码

默认情况下，下列代码都应放入热更新程序集：

- 新玩法；
- 业务状态和业务数据；
- UI 页面逻辑；
- 数值计算；
- 活动、任务、背包等业务模块；
- 可在平台接口之上实现的逻辑；
- Excel 生成配置。

推荐目录：

```text
Assets/Scripts/HotUpdate/
├─ Config/       # 生成文件
├─ UI/           # UI 页面
├─ Utils/        # 热更新通用工具
├─ Workflow/     # 热更新业务入口/流程
├─ Manager/      # 可新增业务 Manager
├─ Model/        # 可新增数据模型
└─ Gameplay/     # 可新增玩法
```

后四个目录目前不一定存在，可按需求创建。

### 1.2 应放入 `Invariable` 的代码

仅当满足以下条件之一时放入不可热更层：

- 必须在 `HotUpdate.dll` 加载前运行；
- 直接依赖微信或抖音 SDK 类型；
- 修改 HybridCLR/YooAsset 初始化；
- 属于所有业务都依赖的基础 Unity 组件；
- 必须挂在首包本地场景或加载面板；
- 热更新层无法安全实现。

修改 `Invariable` 后，应明确知晓：**需要重新发布小游戏基础包。**

### 1.3 应放入 `Editor/MyTools` 的代码

- Excel 导出；
- 资源导入；
- DLL 复制；
- 自动构建；
- 资源检查；
- 编辑器菜单和批处理。

Editor 代码不能被运行时程序集引用。

## 2. 命名和结构约定

当前代码约定：

| 内容 | 约定 |
|---|---|
| 不可热更命名空间 | `Invariable` |
| 热更新命名空间 | `HotUpdate` |
| 私有字段 | `m_` 前缀 |
| 静态缓存字段 | 生成配置使用 `s_` 前缀 |
| UI 页面类名 | 与 Prefab 文件名一致 |
| UI Prefab 地址 | `Prefabs_{Prefab名}` |
| 音频地址 | `Audios_{音频名}` |
| 独立图片地址 | `Png_{文件名}` |
| 图集地址 | `Atlas_{图集名}` |
| DLL 地址 | `MiniGame_{DLL文件名}` |
| UI 挂载节点 | `UI_Root/Canvas_{layer}/Ts_Panel` |
| 事件名 | 当前为字符串，建议使用 `模块_动作` |
| 计时器 key | 必须全局唯一，建议 `类名_对象ID_用途` |

新增类时建议保持：

- 一个主要类型一个文件；
- 文件名与主要类型名一致；
- 公共 API 写 XML 注释；
- 平台差异集中在 `SdkManager`，业务代码不要散落平台宏；
- 业务层尽量通过平台无关的方法调用 `SdkManager`。

## 3. 代码书写规范

本项目新增或修改 C# 脚本时，统一参照 `Assets/Scripts/HotUpdate/UI/MainPanel/MainPanel.cs` 的书写风格。以下规范适用于 `HotUpdate`、`Invariable` 和 Editor 工具代码；自动生成的 `Tab_*.cs` 以生成器输出格式为准。

### 3.1 文件整体结构

脚本内容按以下顺序组织：

1. `using` 引用；
2. 命名空间；
3. 类型声明；
4. 字段；
5. Unity 生命周期函数；
6. 业务方法；
7. 事件回调。

### 3.2 `using` 引用

- 每个命名空间单独占一行；
- 只保留脚本实际使用的引用；
- Unity、项目程序集和第三方库可按功能相邻排列；
- `using` 区域结束后，与 `namespace` 之间保留 **3 个空行**；
- 不在文件中间声明 `using`；
- 除非类型重名或能明显提高可读性，否则不要使用完整限定类型名代替 `using`。

### 3.3 命名规范

| 对象 | 规范 | 示例 |
|---|---|---|
| 命名空间 | PascalCase | `HotUpdate` |
| 类、结构体、枚举 | PascalCase | `MainPanel` |
| 方法 | PascalCase | `PlayBtnAnim` |
| Unity 生命周期函数 | 使用 Unity 原始名称 | `Awake`、`Start`、`OnEnable` |
| 成员字段 | `m_` + camelCase | `m_btnPlay`、`m_tsPlay` |
| 局部变量、参数 | camelCase | `itemIndex`、`callBack` |
| 常量 | PascalCase | `MaxItemCount` |
| 布尔字段 | `m_is`、`m_has`、`m_can` 等语义前缀 | `m_isPlaying` |
| 事件处理方法 | `On` + 对象/行为 + 事件 | `OnPlayGameClick` |

常用 Unity/UI 字段缩写沿用现有工程风格：

| 前缀 | 类型/用途 | 示例 |
|---|---|---|
| `m_btn` | `UIButton` 或按钮 | `m_btnPlay` |
| `m_ts` | `Transform` / `RectTransform` | `m_tsPlay` |
| `m_text` | TextMeshPro 文本 | `m_textTitle` |
| `m_img` | `Image` | `m_imgIcon` |
| `m_rawImg` | `RawImage` | `m_rawImgPreview` |
| `m_go` | `GameObject` | `m_goContent` |
| `m_sli` | `Slider` | `m_sliProgress` |
| `m_scroll` | `ScrollRect` | `m_scrollList` |

命名必须表达业务含义，不使用 `a`、`b`、`temp1`、`obj2` 等无法判断用途的名称。

### 3.4 字段声明与分组

- 字段统一声明在类的顶部、方法之前；
- Inspector 绑定字段沿用当前项目的 `public` + `m_` 风格；
- 同一用途的字段连续排列；
- 不同用途的字段较多时，可使用一个空行分组；
- 字段区域结束后，与第一个方法之间保留 **3 个空行**；
- 字段声明时仅设置明确且安全的默认值，不在字段初始化器中执行复杂逻辑。

### 3.5 组件引用与预制体绑定

所有能够在 Unity Inspector 中配置的组件引用，必须声明为 `public` 成员字段，并在场景或预制体上通过拖拽完成赋值。

推荐写法：

```csharp
public UIButton m_btnPlay;
public RectTransform m_tsPlay;
public TextMeshProUGUI m_textTitle;
```

不推荐在运行时查找已经固定存在于预制体中的组件：

```csharp
// 不推荐：固定组件不应在运行时查找
private void Awake()
{
    m_btnPlay = transform.Find("Btn_Play").GetComponent<UIButton>();
    m_tsPlay = GameObject.Find("UI_Root/Canvas_0/Ts_Panel").GetComponent<RectTransform>();
}
```

必须遵守以下规则：

- UI 控件、动画节点、文本、图片、列表、按钮及其他固定组件引用，统一使用 `public` 字段；
- `public` 组件字段必须使用 `m_` 前缀和对应的类型语义前缀；
- 字段必须在对应场景或 Prefab 的 Inspector 中拖拽赋值；
- 新增或修改字段后，必须打开对应场景或 Prefab 检查引用是否完整；
- 不允许为了减少 Inspector 字段而使用 `GameObject.Find`、`Transform.Find` 或 `GetComponent` 查找固定组件；
- 不允许依赖节点名称和层级路径获取本来可以直接绑定的对象；
- 不允许在 `Update`、循环或高频方法中执行任何 `Find` 查找；
- Prefab 变更时应同时提交脚本和对应 Prefab，避免代码字段与资源绑定不一致。

只有以下少数情况可以使用运行时查找：

1. 对象由代码动态实例化，编辑阶段无法拖拽绑定；
2. 目标对象来自运行时加载的场景或 Prefab，编译时不存在引用关系；
3. 框架级全局根节点需要跨场景定位，例如现有的 `UI_Root`；
4. 第三方 SDK 或框架 API 只能通过名称、路径或类型获取对象；
5. 为兼容旧资源临时补偿缺失引用，并且需求明确要求兼容。

确需运行时查找时，必须同时满足：

- 在代码旁写注释说明不能拖拽绑定的原因；
- 尽量只查找一次，并将结果缓存到成员字段；
- 使用前检查查找结果是否为 `null`；
- 查找失败时输出包含节点名称或路径的明确错误日志；
- 不使用容易变化的深层路径；
- 后续具备 Inspector 绑定条件时，应改回 `public` 字段拖拽赋值。

允许的动态对象示例：

```csharp
public RectTransform m_tsItemParent;
public GameObject m_itemPrefab;

private RectTransform CreateItem()
{
    GameObject item = GameObject.Instantiate(m_itemPrefab, m_tsItemParent);
    return item.GetComponent<RectTransform>(); // 动态实例对象，无法提前拖拽其组件
}
```

即使对象是动态创建的，其**模板 Prefab**和**父节点**仍然必须优先通过 `public` 字段拖拽绑定。

### 3.6 方法排列顺序

类中的方法按以下顺序排列：

1. Unity 生命周期函数：`Awake`、`OnEnable`、`Start`、`Update`、`OnDisable`、`OnDestroy`；
2. 公共业务方法；
3. 私有业务方法；
4. 按钮、事件和异步回调方法。

同一组内按照实际调用流程排列。入口方法调用的业务方法应尽量放在其后方，便于自上而下阅读。

生命周期方法组与业务方法组之间保留 **3 个空行**；同组方法之间保留 **1 个空行**。

### 3.7 缩进、花括号与空格

- 使用 **4 个空格**缩进，不使用 Tab；
- 命名空间、类、方法、条件和循环的左花括号独占下一行；
- 左右花括号必须成对保留，即使代码块当前只有一行；
- 二元运算符、赋值符号和逗号后保留一个空格；
- 方法调用的左括号前不加空格；
- `if`、`for`、`while`、`switch` 等关键字与左括号之间保留一个空格；
- 不在行尾保留多余空格；
- 连续空行数量按照本章分组规则执行，不随意增减。

### 3.8 注释规范

- 业务方法使用中文 XML `<summary>` 注释，说明“做什么”，不逐行复述代码；
- Unity 生命周期函数名称已经明确时，可以不写 XML 注释；
- 公共方法存在参数或返回值时，补充 `<param>`、`<returns>`；
- 行尾注释仅用于解释当前语句中不直观的目的；
- 注释与代码保持同步，逻辑修改后必须同时更新注释；
- 不保留被注释掉的废弃代码，历史实现交由 Git 管理。

示例：

```csharp
/// <summary>
/// 播放开始游戏按钮的动画
/// </summary>
private void PlayBtnAnim()
{
    // ...
}
```

允许的简短行尾注释：

```csharp
GameManager.Instance.InvokeEventCallBack("Launcher_StartGame"); // 销毁热更新面板
```

### 3.9 方法体与职责

- 一个方法只承担一个明确职责；
- 生命周期函数只负责组织调用，不堆积大段业务实现；
- 可独立描述的逻辑提取为私有方法；
- 按钮点击方法使用 `OnXxxClick` 命名；
- 方法较短时不为了形式继续拆分；
- 避免超过 3 层的深层嵌套，优先使用提前返回；
- 对外部输入、资源加载结果和可能为空的对象进行必要校验；
- 注册的事件、按钮监听、计时器和 SDK 回调应在对应生命周期中解除。

`MainPanel.Start` 的组织方式是推荐写法：

```csharp
private void Start()
{
    PlayBGM();
    PlayBtnAnim();
    m_btnPlay.AddClickListener(OnPlayGameClick);
}
```

### 3.10 链式调用与 Lambda

- 简短且语义连续的链式调用可以写在同一行；
- 链式调用包含 Lambda 时，Lambda 代码块另起一行并按层级缩进；
- 单行过长或调用步骤需要分别解释时，应合理换行；
- Lambda 内逻辑超过少量语句时，提取为命名方法；
- 不在复杂 Lambda 中混合资源加载、状态修改和 UI 刷新等多个职责。

示例：

```csharp
m_tsPlay.DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.InSine).OnComplete(() =>
{
    m_tsPlay.DOAnchorPos(new Vector2(0, -500), 1f).SetEase(Ease.OutSine);
});
```

### 3.11 字面量与类型使用

- `float` 字面量使用 `f` 后缀，例如 `1f`、`0.2f`；
- 优先使用明确类型和项目已有类型，不为了缩短代码滥用 `var`；
- 当右侧类型清晰且不会降低可读性时可以使用 `var`；
- 字符串事件名、资源地址等应沿用项目既有格式；
- 同一业务字符串重复使用时，应提取为常量，避免多处拼写不一致；
- 不直接在业务代码中写入密码、正式广告位 ID 或其他敏感配置。
- 文档只规定书写格式，不代表每个页面都必须包含 `Awake`、动画或按钮。没有实际用途的字段、生命周期方法和空方法不得保留。

### 3.12 代码提交前检查

- [ ] `using` 无冗余，且与 `namespace` 之间保留 3 个空行；
- [ ] 字段位于方法之前，并使用正确的 `m_` 及类型语义前缀；
- [ ] 固定组件引用均声明为 `public` 字段，并已在场景或 Prefab 中拖拽赋值；
- [ ] 没有使用 `Find` 或 `GetComponent` 查找本可通过 Inspector 绑定的固定组件；
- [ ] 必要的运行时查找已说明原因，并具有缓存、空值检查和错误日志；
- [ ] 生命周期函数、业务方法、事件回调顺序清晰；
- [ ] 字段区与方法区、生命周期区与业务区之间保留 3 个空行；
- [ ] 业务方法具有准确的中文 XML 注释；
- [ ] 使用 4 空格缩进和换行花括号；
- [ ] 方法职责单一，没有不必要的深层嵌套；
- [ ] 链式调用和 Lambda 排版与现有脚本一致；
- [ ] 事件、按钮监听、计时器和 SDK 回调已对称清理；
- [ ] 没有废弃注释代码、调试残留或敏感信息。

## 4. 新增普通热更新业务脚本

示例目录：

```text
Assets/Scripts/HotUpdate/Gameplay/ExampleFeature.cs
```

示例：

```csharp
using Invariable;

namespace HotUpdate
{
    public class ExampleFeature
    {
        public void Start()
        {
            GameManager.Instance.InvokeEventCallBack(
                "ExampleFeature_Started"
            );
        }
    }
}
```

注意：

1. `HotUpdate` 可以 `using Invariable`；
2. 不要让 `Invariable` 直接引用此类型；
3. 如需从不可热更层调用，应定义稳定接口、事件，或在唯一入口处反射；
4. 如果使用新泛型组合，真机构建前应验证 HybridCLR AOT 泛型支持。

## 5. 新增 UI 页面

假设新增 `InventoryPanel`。

### 5.1 创建脚本

位置：

```text
Assets/Scripts/HotUpdate/UI/InventoryPanel/InventoryPanel.cs
```

示例：

```csharp
using Invariable;
using UnityEngine;



namespace HotUpdate
{
    public class InventoryPanel : UIPanel
    {
        public UIButton m_btnClose;



        private void Start()
        {
            m_btnClose.AddClickListener(OnCloseClick);
        }

        private void OnDestroy()
        {
            m_btnClose?.ReleaseClickListener();
        }



        private void OnCloseClick()
        {
            Close();
        }
    }
}
```

### 5.2 创建 Prefab

位置建议：

```text
Assets/GameAssets/Prefabs/UI/InventoryPanel/InventoryPanel.prefab
```

要求：

- Prefab 文件名必须为 `InventoryPanel`；
- 脚本类名必须为 `InventoryPanel`；
- 命名空间必须为 `HotUpdate`；
- 类必须继承 `UIPanel`；
- Inspector 字段必须完整绑定；
- 若需要弹窗动画，根对象附加 `UIPopup` 并绑定 `m_trans`；
- Prefab 必须位于 YooAsset `Prefabs` 收集目录下。

### 5.3 打开页面

```csharp
HotUpdateUtils.OpenUIPrefabPanel(
    "InventoryPanel",
    0,
    panelObject =>
    {
        var panel = panelObject.GetComponent<InventoryPanel>();
        // 初始化参数
    }
);
```

### 5.4 关闭页面

页面内部：

```csharp
Close();
```

外部按名称：

```csharp
UIManager.Instance.CloseUIPanel("InventoryPanel");
```

### 5.5 UI 页面检查清单

- [ ] 脚本名、类名、Prefab 名一致；
- [ ] 使用 `HotUpdateUtils`，不是 `Invariable.Utils.OpenUIPrefabPanel`；
- [ ] layer 对应节点存在；
- [ ] Inspector 引用完整，所有固定组件均通过 `public` 字段拖拽赋值；
- [ ] 没有使用 `Find` 查找本可直接绑定的 UI 组件；
- [ ] 页面关闭后事件和计时器已取消；
- [ ] 异步资源回调时页面可能已销毁，已处理空引用；
- [ ] 快速重复点击不会实例化重复页面；
- [ ] Prefab 已进入 YooAsset 构建结果；
- [ ] 真机 HybridCLR 能找到脚本类型。

## 6. 新增弹窗或顶层提示

### 6.1 标准提示弹窗

```csharp
HotUpdateUtils.OpenTipsPanel(
    content: "是否继续？",
    btn1: "确定",
    callBack1: OnConfirm,
    btn2: "取消",
    callBack2: OnCancel,
    title: "提示"
);
```

只有一个按钮时将 `btn2` 留空。

### 6.2 飘字

```csharp
HotUpdateUtils.ShowFloatText("操作成功");
```

`FloatTextPanel` 会复用内部 item，但页面本身在 UIManager 中按单例管理。

## 7. 使用事件系统

### 7.1 注册与移除

推荐生命周期对称：

```csharp
private const string EventDataChanged = "Inventory_DataChanged";

private void OnEnable()
{
    GameManager.Instance.AddEventListener(
        EventDataChanged,
        OnDataChanged
    );
}

private void OnDisable()
{
    GameManager.Instance.RemoveEventListener(
        EventDataChanged,
        OnDataChanged
    );
}

private void OnDataChanged(object arg)
{
    var itemId = (int)arg;
}
```

触发：

```csharp
GameManager.Instance.InvokeEventCallBack(
    EventDataChanged,
    itemId
);
```

### 7.2 使用注意

- 参数类型不匹配会在运行时失败；
- 不要用含义相近但拼写不同的事件名；
- 不要在匿名 lambda 注册后尝试用另一个 lambda 移除；
- 页面销毁前必须移除监听；
- 回调中避免直接增删同一个事件的监听集合；
- 高频数据同步不宜全部经过 `object` 事件，可考虑明确接口。

如事件数量增长，建议后续新增：

```text
HotUpdate/Constants/GameEventNames.cs
```

集中定义常量。

## 8. 使用计时器

### 8.1 一次性延迟

```csharp
private const string DelayKey = "InventoryPanel_RefreshDelay";

GameManager.Instance.DelayCallSeconds(
    DelayKey,
    RefreshView,
    0.5f
);
```

清理：

```csharp
private void OnDisable()
{
    GameManager.Instance.CancelInvokeByKey(DelayKey);
}
```

### 8.2 重复调用

```csharp
private const string TimerKey = "Battle_Countdown";

GameManager.Instance.RepeatingCallSeconds(
    TimerKey,
    TickCountdown,
    1f
);
```

停止：

```csharp
GameManager.Instance.CancelInvokeByKey(TimerKey);
```

### 8.3 必须注意的当前行为

- 相同 key 已存在时不会启动新计时；
- 一次性调用完成后 key 仍保留；
- 页面禁用/销毁必须主动调用取消；
- 当前接口为 `async void`；
- 计时使用 `Time.deltaTime` 的重复秒调用受 TimeScale 影响；
- 不要把短生命周期对象直接永久捕获在回调中。

## 9. 加载资源

### 9.1 普通资源

```csharp
YooAssetManager.Instance.AsyncLoadAsset<Sprite>(
    "Png_icon",
    sprite =>
    {
        // 使用资源
    }
);
```

### 9.2 设置图片

独立图片：

```csharp
Utils.SetImage(gameObject, "Icon", "icon");
```

实际地址：

```text
Png_icon
```

图集图片：

```csharp
Utils.SetImage(
    gameObject,
    "Icon",
    "AtlasName/SpriteName"
);
```

实际图集地址：

```text
Atlas_AtlasName
```

### 9.3 设置灰度

```csharp
Utils.SetGray(gameObject, "Icon", true);
Utils.SetGray(gameObject, "Icon", false);
```

### 9.4 场景

```csharp
YooAssetManager.Instance.AsyncLoadScene(
    "Scenes_Battle",
    UnityEngine.SceneManagement.LoadSceneMode.Additive,
    scene =>
    {
        // 场景加载完成
    }
);
```

卸载：

```csharp
YooAssetManager.Instance.UnLoadScene("Scenes_Battle");
```

注意：当前卸载场景会先释放所有普通资源句柄。若新功能依赖跨场景常驻资源，应先改进资源管理策略。

### 9.5 资源加载检查清单

- [ ] 地址与 Collector 地址规则一致；
- [ ] 文件位于 `Assets/GameAssets` 对应目录；
- [ ] 加载失败路径有日志或用户提示；
- [ ] 回调触发时宿主对象仍存活；
- [ ] 不依赖未定义的回调顺序；
- [ ] 场景卸载不会意外释放仍使用的资源；
- [ ] 发布前重新构建并上传 YooAsset 清单和 Bundle。

## 10. 使用音频

播放：

```csharp
AudioManager.Instance.PlayAudio("bgm", true);
```

停止：

```csharp
AudioManager.Instance.StopAudio("bgm");
```

暂停：

```csharp
AudioManager.Instance.PauseAudio("bgm");
```

文件应位于：

```text
Assets/GameAssets/Audios/bgm.*
```

地址由代码生成：

```text
Audios_bgm
```

当前没有 Resume API；再次调用 `PlayAudio` 会在 clip 已加载且未播放时执行 `Play()`。

## 11. 新增或修改 Excel 配置

### 11.1 修改数据

1. 修改 `Excel` 目录中的源表；
2. 不要直接修改 `Tab_*.cs`；
3. 执行菜单：
   `VastStarryRiver/Config/导出Excel配置`；
4. 等待脚本重新编译；
5. 检查生成类字段、类型和数据；
6. 重新导出 HybridCLR DLL；
7. 重新构建 YooAsset。

### 11.2 使用配置

```csharp
var row = Tab_Player.GetConfigByIndex("1_1");
if (row != null)
{
    int level = row.StartLv;
}

foreach (var rune in Tab_RoleRune.GetAllConfigs())
{
    // 使用 rune
}
```

配置类会在首次读取时懒构建，无需预先调用 `Init()`。

### 11.3 修改导表规则

如果需求是支持新字段类型、客户端列筛选或新代码结构，应修改：

```text
Assets/Editor/MyTools/Tools/ConfigTool.cs
```

这是 Editor 工具修改，不属于热更新业务脚本。修改后应使用小型测试表验证：

- 字符串转义；
- 小数点区域设置；
- 空值；
- 数组分隔符；
- C# 关键字字段；
- 重复 Index；
- 多 Sheet；
- 中文表名/字段名。

## 12. 使用平台存储

统一接口：

```csharp
SdkManager.Instance.SetLocalData("PlayerName", name);

string name = SdkManager.Instance.GetLocalData(
    "PlayerName",
    "Default"
);
```

对应实现：

| 环境 | 实现 |
|---|---|
| Editor | `PlayerPrefs` |
| 微信 | `WX.Storage*Sync` |
| 抖音 | `TT.Save` / `TT.LoadSaving` |

存档修改应考虑：

- key 不要随意改名；
- 新字段提供默认值；
- JSON 数据要做版本兼容；
- 不要把服务端可信数据只存本地；
- 大数据量不宜继续只使用字符串接口。

## 13. 新增平台能力

推荐做法：

1. 在 `SdkManager` 添加平台无关的公共方法；
2. 方法内部使用平台宏分支；
3. Editor 分支提供可预测的模拟结果；
4. 热更新业务只调用公共方法；
5. 真机分别验证微信和抖音；
6. 明确回调在成功、失败、取消时是否都能结束。

示例结构：

```csharp
public void Share(Action<bool> callback)
{
#if UNITY_EDITOR
    callback?.Invoke(true);

#elif MINIGAME_SUBPLATFORM_WEIXIN
    // 微信实现

#elif MINIGAME_SUBPLATFORM_DOUYIN
    // 抖音实现

#else
    callback?.Invoke(false);
#endif
}
```

注意：修改 `SdkManager` 属于不可热更修改。

## 14. 修改启动或热更新流程

若需要新增状态节点：

1. 在 `Invariable/Workflow` 新增实现 `IStateNode` 的类；
2. 在 `Launcher.Start` 调用 `stateMachine.AddNode<T>()`；
3. 在前置状态成功后调用 `ChangeState<T>()`；
4. 为失败、重试、取消设计明确状态；
5. 如需跨节点传参，使用黑板；
6. 确保异步回调不会在状态退出后错误切换；
7. 确保加载面板事件仍在 Launcher 销毁前有效。

状态机不会自动调用 `Update()`；当前启动节点都以协程/回调驱动，因此没有依赖 `OnUpdate()`。若新节点需要每帧更新，还必须让宿主持有状态机并在 Unity `Update` 中调用它。

## 15. Prefab 和脚本绑定注意事项

- 热更新 UI 可以通过 Prefab 序列化脚本或运行时按类名补加组件；
- 运行时补加依赖“Prefab 名 = 类名”；
- 字段序列化变化可能导致旧 Prefab 丢引用；
- 重命名脚本或移动命名空间时要检查 Prefab；
- 修改字段类型后要重新打开 Prefab 验证；
- 不要只改脚本而忽略对应 `.prefab`；
- 不要手工编辑 Unity YAML（资源文件文本内容），除非确有必要并能验证引用。

## 16. BUG 修复标准检查清单

### 16.1 通用

- [ ] 已确认复现路径和根因，而非只隐藏异常；
- [ ] 已搜索所有调用方；
- [ ] 未覆盖用户未提交修改；
- [ ] 修改范围符合热更新边界；
- [ ] 对空对象、失败回调、重复点击、快速开关做了处理；
- [ ] 没有新增字符串地址拼写不一致；
- [ ] 没有留下事件、计时器和 SDK 监听；
- [ ] 日志不包含密钥、用户隐私或认证信息。

### 16.2 UI

- [ ] Prefab 绑定完整；
- [ ] 页面名、脚本名一致；
- [ ] 层级正确；
- [ ] 动画完成前关闭/重复打开安全；
- [ ] 异步资源回调时页面仍有效。

### 16.3 热更新

- [ ] `HotUpdate.asmdef` 引用足够；
- [ ] 新类型可被 HybridCLR 加载；
- [ ] AOT 泛型和反射类型已验证；
- [ ] DLL 已重新生成、加密并构建到 YooAsset；
- [ ] 真机不是仍在使用旧缓存清单。

### 16.4 平台

- [ ] Editor、微信、抖音分支均有明确行为；
- [ ] 正确平台宏处于激活状态；
- [ ] SDK 初始化完成后才调用；
- [ ] 成功、失败、关闭、取消均会回调；
- [ ] 真机生命周期切后台/回前台已验证。