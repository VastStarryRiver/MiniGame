using UnityEngine.Networking;

namespace TTSDK
{
    public static class UnityWebRequestCompatibility
    {
#if !PLATFORM_PLAYABLEADS
        public static UnityWebRequest PostWwwForm(string host, string data)
        {
#if UNITY_2022_1_OR_NEWER
            return UnityWebRequest.PostWwwForm(host, data);
#else
            return UnityWebRequest.Post(host, data);
#endif
        }
#endif
    }
}