using UnityEngine;
using System.Collections.Generic;
using YooAsset;



public class Launcher : MonoBehaviour
{
    private EPlayMode m_playMode;
    private GameLoadingPanel m_hotUpdatePanel;



    private void Awake()
    {
#if UNITY_EDITOR
        m_playMode = EPlayMode.EditorSimulateMode;
#else
        m_playMode = EPlayMode.WebPlayMode;
#endif

        Utils.CreateManagerInstance("GameManager");
        Utils.CreateManagerInstance("AudioManager", new string[] { "AudioListener" });
    }

    private void OnEnable()
    {
        GameManager.Instance.AddEventListener("Launcher_ShowTips", ShowTips);
        GameManager.Instance.AddEventListener("Launcher_ShowProgress", ShowProgress);
        GameManager.Instance.AddEventListener("Launcher_StartGame", StartGame);
    }

    private void Start()
    {
        StateMachine stateMachine = new StateMachine(this);

        stateMachine.AddNode<InitializeYooAsset>();
        stateMachine.AddNode<CheckCatalogUpdate>();
        stateMachine.AddNode<CheckResourceUpdates>();
        stateMachine.AddNode<HotUpdateOver>();

        stateMachine.SetBlackboardValue("EPlayMode", m_playMode);

        InitStartGameObject("SceneGameObject");
        InitStartGameObject("UI_Root");

        GameObject go = GameObject.Find("UI_Root/Canvas_0/Ts_Panel/HotUpdatePanel");

        if (go == null)
        {
            Transform parent = GameObject.Find("UI_Root/Canvas_0/Ts_Panel").transform;
            GameObject asset = Resources.Load<GameObject>("LocalAssets/HotUpdatePanel");
            go = GameObject.Instantiate<GameObject>(asset, Vector3.zero, Quaternion.identity, parent);
            go.name = name;
        }

        m_hotUpdatePanel = go.GetComponent<GameLoadingPanel>();

        stateMachine.Play<InitializeYooAsset>();
    }

    private void OnDisable()
    {
        GameManager.Instance.RemoveEventListener("Launcher_ShowTips", ShowTips);
        GameManager.Instance.RemoveEventListener("Launcher_ShowProgress", ShowProgress);
        GameManager.Instance.RemoveEventListener("Launcher_StartGame", StartGame);
    }



    private void InitStartGameObject(string name)
    {
        GameObject go = GameObject.Find(name);

        if (go != null)
        {
            DontDestroyOnLoad(go);
        }
    }

    private void ShowTips(object arg)
    {
        string tips = arg as string;
        m_hotUpdatePanel.SetDes(tips);
    }

    private void ShowProgress(object arg)
    {
        List<long> progress = arg as List<long>;
        m_hotUpdatePanel.SetProgress(progress[0], progress[1], $"{Utils.FormatFileByteSize(progress[0])}/{Utils.FormatFileByteSize(progress[1])}");
    }

    private void StartGame(object arg)
    {
        GameObject.Destroy(m_hotUpdatePanel.gameObject);
        GameObject.Destroy(this);
    }
}