namespace TTSDK
{
    public static class TTSDKType
    {
        public static bool IsMixEngine()
        {
#if TTSDK_MIX_ENGINE
            return true;
#endif
            return false;
        }
    }
}