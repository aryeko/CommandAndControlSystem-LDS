namespace ControlApplication.Core.Networking.Factories
{
    /// <summary>
    /// Gets the Networking APIs
    /// </summary>
    class NetworkFactory : AbstractNetworkFactory
    {
        /// <summary>
        /// Gets NT server API directly
        /// </summary>
        /// <returns></returns>
        public override INtServerApi GetNtServerApi()
        {
            return ServerConnectionManager.Instance;
        }
    }
}