using System;
using System.Collections;
using System.Reflection;
using YooAsset;



namespace Invariable
{
    public class HotUpdateOver : IStateNode
    {
        private StateMachine m_machine = null;



        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(ClearUnusedFiles());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }



        /// <summary>
        /// 清理旧缓存
        /// </summary>
        private IEnumerator ClearUnusedFiles()
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "清理缓存中...");

            ClearCacheFilesOperation operation1 = YooAssetManager.Instance.Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedManifestFiles);
            yield return operation1;

            ClearCacheFilesOperation operation2 = YooAssetManager.Instance.Package.ClearCacheFilesAsync(EFileClearMode.ClearUnusedBundleFiles);
            yield return operation2;

            InitializeOperationSystem();
        }

        /// <summary>
        /// 初始化运行系统
        /// </summary>
        private void InitializeOperationSystem()
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "初始化游戏中...");
            SdkManager.Instance.InitSDK(() =>
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "同步数据中...");
                CloudManager.Instance.InitCloudData(() =>
                {
                    YooAssetManager.Instance.PreLoadDll((hotUpdateAss) =>
                    {
                        GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "即将进入游戏...");
                        Type type = hotUpdateAss.GetType("HotUpdate.StartGame");
                        MethodInfo methodInfo = type.GetMethod("Play", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        methodInfo.Invoke(null, null);
                    });
                });
            });
        }
    }
}