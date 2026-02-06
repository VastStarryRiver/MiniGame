using YooAsset;
using System.Collections;
using System;
using System.Reflection;



namespace Invariable
{
    public class HotUpdateOver : IStateNode
    {
        private StateMachine m_machine;

        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(ClearUnusedFiles());
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }

        /// <summary>
        /// 清理旧缓存
        /// </summary>
        private IEnumerator ClearUnusedFiles()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "清理旧缓存");

            var operation1 = YooAssetManager.Instance.Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
            yield return operation1;

            var operation2 = YooAssetManager.Instance.Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            yield return operation2;

            InitializeOperationSystem();
        }

        /// <summary>
        /// 初始化运行系统
        /// </summary>
        private void InitializeOperationSystem()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化运行系统");
            SdkManager.Instance.InitSDK(() =>
            {
                YooAssetManager.Instance.PreLoadDll((hotUpdateAss) =>
                {
                    GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "开始游戏");
                    Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
                    MethodInfo methodInfo = type.GetMethod("Play", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    methodInfo.Invoke(null, null);
                });
            });
        }
    }
}