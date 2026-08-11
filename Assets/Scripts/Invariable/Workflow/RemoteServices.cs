using YooAsset;



namespace Invariable
{
    public class RemoteServices : IRemoteServices
    {
        private readonly string DefaultHostServer;
        private readonly string FallbackHostServer;



        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            DefaultHostServer = defaultHostServer;
            FallbackHostServer = fallbackHostServer;
        }



        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{DefaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{FallbackHostServer}/{fileName}";
        }
    }
}