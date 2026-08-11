using System.Diagnostics;



namespace Invariable
{
    public static class GameLog
    {
        /// <summary>
        /// Info 级日志（仅编辑器环境编译，包内剔除）
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void Info(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <summary>
        /// Error 级日志（始终输出）
        /// </summary>
        public static void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}