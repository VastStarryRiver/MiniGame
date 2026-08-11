using System.Collections;
using YooAsset;



namespace Invariable
{
    public class CheckCatalogUpdate : IStateNode
    {
        private StateMachine m_machine = null;



        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(RequestPackageVersion());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }



        /// <summary>
        /// 获取资源版本
        /// </summary>
        private IEnumerator RequestPackageVersion()
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "检查清单中...");

            RequestPackageVersionOperation operation = YooAssetManager.Instance.Package.RequestPackageVersionAsync(false);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "清单检查完成");
                GameManager.Instance.StartCoroutine(UpdatePackageManifest(operation.PackageVersion));
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "清单检查失败，请检查网络后重启游戏");
            }
        }

        /// <summary>
        /// 更新资源清单
        /// </summary>
        /// <param name="packageVersion">资源包版本</param>
        private IEnumerator UpdatePackageManifest(string packageVersion)
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "同步清单信息中...");

            UpdatePackageManifestOperation operation = YooAssetManager.Instance.Package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "清单信息同步完成");
                m_machine.ChangeState<CheckResourceUpdates>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "清单信息同步失败，请检查网络后重启游戏");
            }
        }
    }
}