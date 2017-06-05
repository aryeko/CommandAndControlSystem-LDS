namespace ControlApplication.Core.Networking
{
    public class NetworkClientsFactory
    {
        private static INtServerApi _ntServerInstance;

        public static INtServerApi GetNtServer(bool cachingSupport = true)
        {
            return _ntServerInstance ?? Insanciate(cachingSupport);
        }

        private static INtServerApi Insanciate(bool cachingSupport)
        {
            return (_ntServerInstance = cachingSupport
                ? (INtServerApi) new ServerProxy(ServerConnectionManager.Instance)
                : ServerConnectionManager.Instance);
        }
    }
}