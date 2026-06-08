using Invariable;



namespace HotUpdate
{
    public class StartGame
    {
        /// <summary>
        /// 开始游戏
        /// </summary>
        public static void Play()
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "开始游戏");
            HotUpdateUtils.OpenUIPrefabPanel("MainPanel", 0);
        }
    }
}