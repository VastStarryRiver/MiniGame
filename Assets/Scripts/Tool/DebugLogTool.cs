using System;
using System.IO;
using UnityEngine;

public static class DebugLogTool
{
    public static void DebugRedLog(string log)
    {
        Debug.Log("<color=#FF2D00>" + log + "</color>");
    }

    public static void DebugYellowLog(string log)
    {
        Debug.Log("<color=#FFFF00>" + log + "</color>");
    }

    public static void DebugBlueLog(string log)
    {
        Debug.Log("<color=#002EFF>" + log + "</color>");
    }

    public static void DebugGreenLog(string log)
    {
        Debug.Log("<color=#00FF11>" + log + "</color>");
    }

    public static void DebugBlackLog(string log)
    {
        Debug.Log("<color=#000000>" + log + "</color>");
    }
}