namespace ControlApplication.Core.Networking
{
    public class NetworkClientsFactory
    {
        private static INtServerApi _ntServerInstance;

        private static IGscanClientsApi _gscanClientsInstance;

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

        public static IGscanClientsApi GetGscanClientsApi()
        {
            return _gscanClientsInstance ?? (_gscanClientsInstance = new GscansClientsManager());
        }
    }
}