using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    /// <summary>
    /// A class that manages the Server connection and creates a single instance of <see cref="ServerConnectionManager"/> (Singelton)
    /// The class provides a detailed API for sessioned communication with the Server.
    /// </summary>
    public class ServerConnectionManager : IDisposable
    {
        /// <summary>
        /// The Remote Server Path
        /// </summary>
        private readonly Uri RemoteServerPath = new Uri("http://localhost:5000");

        /// <summary>
        /// WebClient for raw network communication
        /// </summary>
        private CookieAwareWebClient WebClient { get; }

        /// <summary>
        /// A single instance of <see cref="ServerConnectionManager"/> (Singelton)
        /// </summary>
        private static ServerConnectionManager _instance;

        /// <summary>
        /// Private constructor which will avoid from an external class to create another instance of <see cref="ServerConnectionManager"/>
        /// </summary>
        private ServerConnectionManager()
        {
            WebClient = new CookieAwareWebClient();
        }

        /// <summary>
        /// Creates an instance of <see cref="ServerConnectionManager"/> or returns the existing instance
        /// </summary>
        /// <returns>Returns single instance of <see cref="ServerConnectionManager"/> as long as the app is running</returns>
        public static ServerConnectionManager GetInstance()
        {
            return _instance ?? (_instance = new ServerConnectionManager());
        }

        /// <summary>
        /// User login authentication 
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Returns whether the user is authorizes or not</returns>
        public bool Login(string username, string password)
        {
            var postData = new NameValueCollection { { "username", username }, { "password", password } };

            var response = WebClient.UploadValues(new Uri(RemoteServerPath, "login") , postData);

            return Encoding.Default.GetString(response).Contains("SUCCESS");
        }

        /// <summary>
        /// Implement <see cref="IDisposable"/> interface to free unmanaged resources. i.e. open sockets.
        /// </summary>
        public void Dispose()
        {
            WebClient.Dispose();
        }
    }
}
