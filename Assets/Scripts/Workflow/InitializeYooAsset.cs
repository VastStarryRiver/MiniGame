using YooAsset;
using System.Collections;
using WeChatWASM;



public class InitializeYooAsset : IStateNode
{
    private StateMachine m_machine;

    public void OnCreate(StateMachine machine)
    {
        m_machine = machine;
    }

    public void OnEnter()
    {
        InitializeSystem();
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

    /// <summary>
    /// 初始化YooAsset资源管理系统
    /// </summary>
    private void InitializeSystem()
    {
        if (YooAssets.Initialized)
        {
            m_machine.ChangeState<HotUpdateOver>();
            return;
        }

        YooAssetManager.Instance.SetWebInfo();

        GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化资源管理系统");

        YooAssets.Initialize();

        YooAssets.SetOperationSystemMaxTimeSlice(1000);

        GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化资源管理系统，成功！");

        ResourcePackage package = YooAssetManager.Instance.Package;

        GameManager.Instance.StartCoroutine(InitializePackage(package));
    }

    /// <summary>
    /// 初始化Package
    /// </summary>
    private IEnumerator InitializePackage(ResourcePackage package)
    {
        GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package");

        EPlayMode playMode = (EPlayMode)m_machine.GetBlackboardValue("EPlayMode");

        InitializationOperation initOperation = null;

        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var buildResult = EditorSimulateModeHelper.SimulateBuild(YooAssetManager.Instance.PackageName);
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

            EditorSimulateModeParameters createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = fileSystemParams;

            initOperation = package.InitializeAsync(createParameters);
        }
        else if (playMode == EPlayMode.WebPlayMode)
        {
            string defaultHostServer = ConfigUtils.CDNPath;
            string fallbackHostServer = defaultHostServer;
            var remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);

            WebPlayModeParameters createParameters = new WebPlayModeParameters();
            string packageRoot = $"{WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE/yoo";
            createParameters.WebServerFileSystemParameters = WechatFileSystemCreater.CreateFileSystemParameters(packageRoot, remoteServices, null);

            initOperation = package.InitializeAsync(createParameters);
        }

        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package，成功！");
            m_machine.ChangeState<CheckCatalogUpdate>();
        }
        else
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package，失败！");
        }
    }
}