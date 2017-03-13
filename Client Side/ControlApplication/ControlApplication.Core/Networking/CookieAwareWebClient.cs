using System;
using System.Net;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// Extending <see cref="WebClient"/> class with cookie container in order to maintain secured sessions 
    /// </summary>
    public class CookieAwareWebClient : WebClient
    {
        /// <summary>
        /// The cookie container
        /// </summary>
        private readonly CookieContainer mContainer = new CookieContainer();

        /// <summary>
        /// Overriding this function in order to add or store 
        /// the cookie (if exists) for each web request.
        /// </summary>
        /// <param name="address">Server's address - will be set by <see cref="WebClient"/> public API</param>
        /// <returns>WebRequest with an embedded cookie</returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = mContainer;
            }
            return request;
        }
    }
}
