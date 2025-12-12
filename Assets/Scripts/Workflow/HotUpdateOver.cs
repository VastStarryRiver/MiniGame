using System;
using System.Reflection;



public class HotUpdateOver : IStateNode
{
    private StateMachine m_machine;

    public void OnCreate(StateMachine machine)
    {
        m_machine = machine;
    }

    public void OnEnter()
    {
        InitializeOperationSystem();
    }

    public void OnExit()
    {

    }

    public void OnUpdate()
    {

    }

    /// <summary>
    /// 初始化运行系统
    /// </summary>
    private void InitializeOperationSystem()
    {
        GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化运行系统");
        SdkManager.Instance.InitMiniGameSDK(StartGame);
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "开始游戏");
        Utils.OpenUIPrefabPanel("UI/Workflow/LoginPanel", 0);
    }
}