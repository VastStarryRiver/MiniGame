using UnityEngine;
using UnityEngine.UI;



public class Launcher : MonoBehaviour
{
    private Slider m_sliProgress;
    private float m_progress;



    private void Awake()
    {
        DontDestroyOnLoad(GameObject.Find("UI_Root"));

        m_sliProgress = GameObject.Find("UI_Root/Canvas_3/Ts_Panel/LoadingPanel/Sli_Progress").GetComponent<Slider>();

        UIManager.Instance.Play();
    }

    private void Start()
    {
        SdkManager.Instance.InitSDK(Play);
    }

    private void Update()
    {
        if (m_sliProgress != null && m_progress < 6)
        {
            m_progress += Time.deltaTime;
            m_sliProgress.value = m_progress / 6;

            if (m_progress >= 6)
            {
                m_sliProgress = null;
                GameObject.Destroy(GameObject.Find("UI_Root/Canvas_3/Ts_Panel/LoadingPanel"));
            }
        }
    }



    private void Play()
    {

    }
}