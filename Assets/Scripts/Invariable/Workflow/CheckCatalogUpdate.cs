using YooAsset;
using System.Collections;



namespace Invariable
{
    public class CheckCatalogUpdate : IStateNode
    {
        private StateMachine m_machine;

        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(RequestPackageVersion());
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }

        /// <summary>
        /// 获取资源版本
        /// </summary>
        private IEnumerator RequestPackageVersion()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "获取资源版本");

            var operation = YooAssetManager.Instance.Package.RequestPackageVersionAsync(false);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "获取资源版本，成功！");
                GameManager.Instance.StartCoroutine(UpdatePackageManifest(operation.PackageVersion));
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "获取资源版本，失败！");
            }
        }

        /// <summary>
        /// 更新资源清单
        /// </summary>
        private IEnumerator UpdatePackageManifest(string packageVersion)
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "更新资源清单");

            var operation = YooAssetManager.Instance.Package.UpdatePackageManifestAsync(packageVersion);
            yield return operation;

            if (operation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "更新资源清单，成功！");
                m_machine.ChangeState<CheckResourceUpdates>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "更新资源清单，失败！");
            }
        }
    }
}