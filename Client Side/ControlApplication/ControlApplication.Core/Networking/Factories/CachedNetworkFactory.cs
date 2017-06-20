namespace ControlApplication.Core.Networking.Factories
{
    /// <summary>
    /// Gets the Networking APIs with catching support
    /// </summary>
    class CachedNetworkFactory : AbstractNetworkFactory
    {
        /// <summary>
        /// Gets NT server API with catching support
        /// </summary>
        /// <returns></returns>
        public override INtServerApi GetNtServerApi()
        {
            return NtServerInstance ?? (NtServerInstance = new ServerProxy(ServerConnectionManager.Instance));
        }
    }
}