namespace ControlApplication.Core.Networking.Factories
{
    /// <summary>
    /// Abstract network factory which supplies the networking APIs
    /// </summary>
    abstract class AbstractNetworkFactory
    {
        /// <summary>
        /// The NT server instance
        /// </summary>
        protected INtServerApi NtServerInstance;

        /// <summary>
        /// The Gscan client instance
        /// </summary>
        protected IGscanClientsApi GscanClientsInstance;

        /// <summary>
        /// Gets NT server API
        /// </summary>
        /// <returns></returns>
        public abstract INtServerApi GetNtServerApi();

        /// <summary>
        /// Gets Gscan client API
        /// </summary>
        /// <returns></returns>
        public virtual IGscanClientsApi GetGscanClientsApi()
        {
            return GscanClientsInstance ?? (GscanClientsInstance = new GscansClientsManager());
        }
    }
}