using ControlApplication.Core.Networking.Factories;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// Class which exposes the networking APIs
    /// </summary>
    public static class Networking
    {
        /// <summary>
        /// The network factory being used
        /// </summary>
        private static AbstractNetworkFactory _abstractNetworkFactory;
        
        /// <summary>
        /// Gets the NT server API
        /// </summary>
        /// <param name="cachingSupport">flag which indicates whether to use catch or not</param>
        /// <returns></returns>
        public static INtServerApi GetNtServer(bool cachingSupport = true)
        {
            Insanciate(cachingSupport);

            return _abstractNetworkFactory.GetNtServerApi();
        }

        /// <summary>
        /// Gets the Gscan client API
        /// </summary>
        /// <returns></returns>
        public static IGscanClientsApi GetGscanClientsApi()
        {
            Insanciate(true);

            return _abstractNetworkFactory.GetGscanClientsApi();
        }

        /// <summary>
        /// Instanciating the network factory
        /// </summary>
        /// <param name="cachingSupport">flag which indicates whether to use catch or not</param>
        private static void Insanciate(bool cachingSupport)
        {
            _abstractNetworkFactory = cachingSupport ? new CachedNetworkFactory() : (AbstractNetworkFactory)new NetworkFactory();
        }
    }
}