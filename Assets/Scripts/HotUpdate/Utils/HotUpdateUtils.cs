using Invariable;
using System;



namespace HotUpdate
{
    public class HotUpdateUtils
    {
        /// <summary>
        /// 打开提示弹窗
        /// </summary>
        public static void OpenTipsPanel(string content, string btn1, Action callBack1 = null, string btn2 = "", Action callBack2 = null, string title = "")
        {
            Utils.OpenUIPrefabPanel("TipsPanel", 2, (obj) =>
            {
                TipsPanel tipsPanel = obj.GetComponent<TipsPanel>();
                tipsPanel.ShowInfo(content, btn1, btn2, callBack1, callBack2, title);
            });
        }

        /// <summary>
        /// 显示浮动提示文本
        /// </summary>
        public static void ShowFloatText(string text)
        {
            Utils.OpenUIPrefabPanel("FloatTextPanel", 3, (obj) =>
            {
                obj.GetComponent<FloatTextPanel>().ShowInfo(text);
            });
        }
    }
}