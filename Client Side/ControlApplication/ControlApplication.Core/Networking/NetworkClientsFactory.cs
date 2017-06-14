namespace ControlApplication.Core.Networking
{
    public class NetworkClientsFactory
    {
        private static INtServerApi _ntProxyServerInstance;
        private static INtServerApi _ntRealServerInstance;

        public static INtServerApi GetNtServer(bool cachingSupport = true)
        {
            return Insanciate(cachingSupport);
        }

        private static INtServerApi Insanciate(bool cachingSupport)
        {
            if (cachingSupport)
                return _ntProxyServerInstance = _ntProxyServerInstance ?? new ServerProxy(ServerConnectionManager.Instance);
            return _ntRealServerInstance = _ntRealServerInstance ?? ServerConnectionManager.Instance;
        }
    }
}