using System.IO;
using UnityEngine;



public class DebugLogTool
{
    public static void DebugRedLog(string log)
    {
        Debug.Log($"<color=#FF2D00>{log}</color>");
    }

    public static void DebugYellowLog(string log)
    {
        Debug.Log($"<color=#FFFF00>{log}</color>");
    }

    public static void DebugBlueLog(string log)
    {
        Debug.Log($"<color=#002EFF>{log}</color>");
    }

    public static void DebugGreenLog(string log)
    {
        Debug.Log($"<color=#00FF11>{log}</color>");
    }

    public static void DebugBlackLog(string log)
    {
        Debug.Log($"<color=#000000>{log}</color>");
    }

    public static void ShowDebugErrorLog(string logString, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR && !UNITY_WEBGL
            if (type == LogType.Error || type == LogType.Exception)
            {
                string text = logString + "\n" + stackTrace;

                using (FileStream lastFileStream = new FileStream(ConfigUtils.m_localRootPath + "Error.txt", FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(lastFileStream))
                    {
                        string lastLog = streamReader.ReadToEnd();

                        if (!string.IsNullOrEmpty(lastLog))
                        {
                            text = lastLog + "\n\n\n" + text;
                        }
                    }
                }

                using (FileStream currFileStream = new FileStream(ConfigUtils.m_localRootPath + "Error.txt", FileMode.Create))
                {
                    using (StreamWriter streamWriter = new StreamWriter(currFileStream))
                    {
                        streamWriter.Write(text);
                    }
                }
            }
#endif
    }

    public static void InitDebugErrorLog()
    {
#if !UNITY_EDITOR && !UNITY_WEBGL
        ConfigUtils.InitDirectory(ConfigUtils.m_localRootPath);

        using (FileStream fileStream = new FileStream(ConfigUtils.m_localRootPath + "Error.txt", FileMode.Create))
        {
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                streamWriter.Write("");
            }
        }

        Application.logMessageReceived += ShowDebugErrorLog;
#endif
    }
}