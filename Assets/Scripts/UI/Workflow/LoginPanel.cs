using UnityEngine;



public class LoginPanel : UIPanel
{
    private void Awake()
    {
        Utils.PlayAnimation(gameObject, null, "Play", WrapMode.Once, () =>
        {
            Utils.SetImage(gameObject, "parent/Img_State1", "Atlas02/02_rwtx5");
            Utils.SetImage(gameObject, "parent/Img_State2", "Atlas02/02_rwtx6");

            Utils.SetGray(gameObject, "parent/Img_State2");

            Utils.SetText(gameObject, "parent/Text_Name", "Ãû×Ö£º<color=#1BB25F>£¿£¿£¿</color>");
        });
    }

    private void Start()
    {
        GameManager.Instance.InvokeEventCallBack("Launcher_StartGame");
    }
}