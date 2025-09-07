using UnityEngine;
using UnityEngine.TextCore;
using TMPro;



public class Launcher : MonoBehaviour
{
    private void Awake()
    {
        GameObject objUIRoot = GameObject.Find("UI_Root");
        DontDestroyOnLoad(objUIRoot);
        //MessageNetManager.Instance.Play();
        UIManager.Instance.Play();
    }

    private void Start()
    {
        SdkManager.Instance.InitSDK(Play);
    }

    private void OnDestroy()
    {
        //MessageNetManager.Instance.Stop();
    }



    private void Play()
    {

    }
}