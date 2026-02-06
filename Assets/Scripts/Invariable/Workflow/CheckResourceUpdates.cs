using YooAsset;
using System.Collections;
using System.Collections.Generic;



namespace Invariable
{
    public class CheckResourceUpdates : IStateNode
    {
        private StateMachine m_machine;

        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(CheckForResourceUpdates());
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }



        /// <summary>
        /// 检查资源更新
        /// </summary>
        private IEnumerator CheckForResourceUpdates()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "检查资源更新");

            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            ResourceDownloaderOperation downloader = YooAssetManager.Instance.Package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "检查资源更新，成功！");

            if (downloader.TotalDownloadCount == 0)
            {
                m_machine.ChangeState<HotUpdateOver>();
                yield break;
            }

            GameManager.Instance.StartCoroutine(DownloadUpdates(downloader));
        }

        /// <summary>
        /// 下载资源
        /// </summary>
        private IEnumerator DownloadUpdates(ResourceDownloaderOperation downloader)
        {
            downloader.DownloadFinishCallback = OnDownloadFinishFunction;
            downloader.DownloadErrorCallback = OnDownloadErrorFunction;
            downloader.DownloadUpdateCallback = OnDownloadUpdateFunction;
            downloader.DownloadFileBeginCallback = OnDownloadFileBeginFunction;
            downloader.BeginDownload();
            yield return downloader;
        }

        /// <summary>
        /// 当下载器结束（无论成功或失败）
        /// </summary>
        /// <param name="data"></param>
        private void OnDownloadFinishFunction(DownloaderFinishData data)
        {
            if (data.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "下载资源，成功！");
                m_machine.ChangeState<HotUpdateOver>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "下载资源，失败！");
            }
        }

        /// <summary>
        /// 当下载器发生错误
        /// </summary>
        /// <param name="data"></param>
        private void OnDownloadErrorFunction(DownloadErrorData data)
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "下载资源，失败！");
        }

        /// <summary>
        /// 当下载进度发生变化
        /// </summary>
        /// <param name="data"></param>
        private void OnDownloadUpdateFunction(DownloadUpdateData data)
        {
            List<long> progress = new List<long>() { data.CurrentDownloadBytes, data.TotalDownloadBytes };
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowProgress", progress);
        }

        /// <summary>
        /// 当开始下载某个文件
        /// </summary>
        /// <param name="data"></param>
        private void OnDownloadFileBeginFunction(DownloadFileData data)
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "下载资源中");
        }
    }
}