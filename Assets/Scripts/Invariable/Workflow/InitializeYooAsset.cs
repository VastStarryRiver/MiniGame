using System.Collections;
using YooAsset;



namespace Invariable
{
    public class InitializeYooAsset : IStateNode
    {
        private StateMachine m_machine = null;



        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            InitializeSystem();
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
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

            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "游戏加载中...");

            YooAssets.Initialize();

            YooAssets.SetOperationSystemMaxTimeSlice(1000);

            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "游戏加载完成");

            ResourcePackage package = YooAssetManager.Instance.Package;

            GameManager.Instance.StartCoroutine(InitializePackage(package));
        }

        /// <summary>
        /// 初始化Package
        /// </summary>
        /// <param name="package">资源包</param>
        private IEnumerator InitializePackage(ResourcePackage package)
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "资源加载中...");

            EPlayMode playMode = (EPlayMode)m_machine.GetBlackboardValue("EPlayMode");

            InitializationOperation initOperation = null;

            if (playMode == EPlayMode.EditorSimulateMode)
            {
                PackageInvokeBuildResult buildResult = EditorSimulateModeHelper.SimulateBuild(YooAssetManager.Instance.PackageName);
                string packageRoot = buildResult.PackageRootDirectory;
                FileSystemParameters fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

                EditorSimulateModeParameters createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = fileSystemParams;

                initOperation = package.InitializeAsync(createParameters);
            }
            else if (playMode == EPlayMode.WebPlayMode)
            {
                string defaultHostServer = $"{ConfigUtils.CDNPath}/yoo";
                string fallbackHostServer = defaultHostServer;
                RemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);

                WebPlayModeParameters createParameters = new WebPlayModeParameters();

                SdkManager.Instance.InitializeYooAsset(ref createParameters, remoteServices);

                initOperation = package.InitializeAsync(createParameters);
            }

            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "资源加载完成");
                m_machine.ChangeState<CheckCatalogUpdate>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "资源加载失败，请检查网络后重启游戏");
            }
        }
    }
}