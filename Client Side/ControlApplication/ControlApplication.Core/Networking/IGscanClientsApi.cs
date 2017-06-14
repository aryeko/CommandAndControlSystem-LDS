using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    public interface IGscanClientsApi
    {
        bool TryStartHostedNetwork();

        bool TryStopHostedNetwork();

        string GetHostedNetwordSsid();

        List<Gscan> GetConnectedDevices();

        List<Detection> GetDeviceDetections(Tuple<PhysicalAddress, IPAddress> devices, Area activeArea);
    }
}
