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
            Utils.OpenUIPrefabPanel("MainPanel", 0);
        }
    }
}
