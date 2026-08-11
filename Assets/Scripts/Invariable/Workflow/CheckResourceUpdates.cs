using System.Collections;
using YooAsset;



namespace Invariable
{
    public class CheckResourceUpdates : IStateNode
    {
        private StateMachine m_machine = null;
        private DownloadProgressInfo m_progressInfo = new DownloadProgressInfo();



        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            GameManager.Instance.StartCoroutine(CheckForResourceUpdates());
        }

        public void OnUpdate()
        {
        }

        public void OnExit()
        {
        }



        /// <summary>
        /// 检查资源更新
        /// </summary>
        private IEnumerator CheckForResourceUpdates()
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "检查更新中...");

            int downloadingMaxNum = 10;
            int failedTryAgain = 3;
            ResourceDownloaderOperation downloader = YooAssetManager.Instance.Package.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);

            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "更新检查完成");

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
        /// <param name="downloader">资源下载器</param>
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
        /// <param name="data">下载结束数据</param>
        private void OnDownloadFinishFunction(DownloaderFinishData data)
        {
            if (data.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "更新完成");
                m_machine.ChangeState<HotUpdateOver>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "更新失败，请检查网络后重启游戏");
            }
        }

        /// <summary>
        /// 当下载器发生错误
        /// </summary>
        /// <param name="data">下载错误数据</param>
        private void OnDownloadErrorFunction(DownloadErrorData data)
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "更新失败，请检查网络后重启游戏");
        }

        /// <summary>
        /// 当下载进度发生变化
        /// </summary>
        /// <param name="data">下载进度数据</param>
        private void OnDownloadUpdateFunction(DownloadUpdateData data)
        {
            m_progressInfo.CurrentBytes = data.CurrentDownloadBytes;
            m_progressInfo.TotalBytes = data.TotalDownloadBytes;
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowProgress, m_progressInfo);
        }

        /// <summary>
        /// 当开始下载某个文件
        /// </summary>
        /// <param name="data">下载文件数据</param>
        private void OnDownloadFileBeginFunction(DownloadFileData data)
        {
            GameManager.Instance.InvokeEventCallBack(InvariableConst.Event_Launcher_ShowTips, "正在更新中...");
        }
    }
}