using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsNativeWifi;

namespace ControlApplication.Core.Networking
{
    public class HostedNetwork : IDisposable
    {
        private readonly WlanClient _wlanClient;

        private readonly string _hostedNetworkSsid;
        public HostedNetwork()
        {
            _wlanClient = new WlanClient();
            //TODO: Set HostedNetwork SSID dynamicly by unit
            _hostedNetworkSsid = "FIELD_UNIT";
        }

        private WlanClient.WlanInterface DefaultInterface => _wlanClient.Interfaces[0];

        /// <summary>
        /// Converts a 802.11 SSID to a string.
        /// </summary>
        private string GetStringForSsid(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        /// <summary>
        /// Converts a string to 802.11 SSID.
        /// </summary>
        private Wlan.Dot11Ssid GetSSIDForString(string ssid)
        {
            Wlan.Dot11Ssid dot11Ssid = new Wlan.Dot11Ssid();
            dot11Ssid.SSID = new byte[32];
            dot11Ssid.SSIDLength = (uint)ssid.Length;

            var ssidBytes = Encoding.ASCII.GetBytes(ssid);

            Array.Copy(ssidBytes, dot11Ssid.SSID, ssidBytes.Length);

            return dot11Ssid;
        }

        public void StartHostedNetwork()
        {
            Wlan.Dot11Ssid ssid = GetSSIDForString(_hostedNetworkSsid);
            DefaultInterface.ConfigureHostedNetwork(ssid);
            DefaultInterface.EnableHostedNetwork();
            DefaultInterface.StartHostedNetwork();
        }

        public void Dispose()
        {
            DefaultInterface.StopHostedNetwork();
        }
    }
}
