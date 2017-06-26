using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using ControlApplication.Core.Contracts;
using Newtonsoft.Json;

namespace ControlApplication.Core.Networking
{
    public class GscansClientsManager : IGscanClientsApi, IDisposable
    {
        private readonly WebClient webClient;

        private const string IdKey = "_ID";
        private const string ScanTimeKey = "scan_time";
        private const string SuspectIdKey = "id_num";
        private const string PlateIdKey = "plate_num";
        private const string MaterialNameKey = "material_detected";
        private const string ScanResultKey = "scan_result";

        /// <summary>
        /// HostedNetwork API
        /// </summary>
        private readonly HostedNetwork hostedNetwork;

        public GscansClientsManager()
        {
            hostedNetwork = new HostedNetwork();
            webClient = new WebClient();
        }

        public bool TryStartHostedNetwork()
        {
            try
            {
                hostedNetwork.StartHostedNetwork();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryStopHostedNetwork()
        {
            try
            {
                hostedNetwork.StopHostedNetwork();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetHostedNetwordSsid()
        {
            return hostedNetwork.HostedNetworkSsid;
        }

        public List<Detection> GetDeviceDetections(Gscan device, Area activeArea)
        {
            var uri = new Uri($"http://{device.IpAddress}:8080/getall");
            var result = webClient.DownloadString(uri);

            var detectionKey = "detectionData";
            var ramanPathKey = "ramanPath";

            var pattern = $"(?<{detectionKey}>{{.*?}})\n<a href=\"(?<{ramanPathKey}>/\\w+)\">.+?</a>";
            var matches = Regex.Matches(result, pattern);

            var detectionsList = new List<Detection>();
            foreach (Match match in matches)
            {
                var str = ExtractGscanDetectionValue(ScanTimeKey, match.Groups[detectionKey].Value);
                DateTime dateTime = new DateTime(1970, 1, 1).AddMilliseconds(double.Parse(str));
                string ramanOutput = GetRaman(device, match.Groups[ramanPathKey].Value); //TODO: Get actual link
                var material = Networking.GetNtServer().GetMaterial(name: ExtractGscanDetectionValue(MaterialNameKey, match.Groups[detectionKey].Value));
                var detection = new Detection(dateTime, material[0], activeArea.RootLocation, activeArea, ExtractGscanDetectionValue(SuspectIdKey, match.Groups[detectionKey].Value), ExtractGscanDetectionValue(PlateIdKey, match.Groups[detectionKey].Value), device.PhysicalAddress.ToString(), ramanOutput);

                detectionsList.Add(detection);
            }

            return detectionsList;
        }

        //TODO: consider how to save raman in DB
        private string GetRaman(Gscan device, string value)
        {
            string path = $@"C:\temp\LDS\Raman\{device.PhysicalAddress}\{value}.esp";
            if (!System.IO.File.Exists(path))
            {
                var uri = new Uri($"http://{device.IpAddress}:8080{value}");
                var result = webClient.DownloadString(uri);
                return result;
            }

            return path;
        }

        private string ExtractGscanDetectionValue(string key, string input)
        {
            var pattern = $"\"{key}\":\"(.*?)\"";

            var match = Regex.Match(input, pattern);

            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public List<Gscan> GetConnectedDevices()
        {
            var devicesMacs = GetHostedNetworkConnectedDevices();
            var devicesIps = GetIpsFromPhisicalAdresses(devicesMacs);

            return devicesMacs.Select((t, i) => new Gscan(t, devicesIps[i])).ToList();
        }

        private List<IPAddress> GetIpsFromPhisicalAdresses(List<PhysicalAddress> physicalAddresses)
        {
            var ipPattern = @"(\d+\.\d+\.\d+\.\d+)\s+{0}";
            var output = RunShellCommand("arp", "-a");

            var ipAdresses = new List<IPAddress>();

            foreach (var physicalAddress in physicalAddresses)
            {
                var mac = Regex.Replace(physicalAddress.ToString(), ".{2}", "$0-").Remove(17);
                Logger.Log($"Getting ip for mac: {physicalAddress}", GetType().Name);
                var ipMatch = Regex.Match(output, string.Format(ipPattern, mac), RegexOptions.IgnoreCase);
                ipAdresses.Add(IPAddress.Parse(ipMatch.Groups[1].Value));
            }

            return ipAdresses;
        }

        private List<PhysicalAddress> GetHostedNetworkConnectedDevices()
        {
            string statusKey = "Status";
            string numnerOfClientsKey = "Number of clients";
            var linePattern = @"{0}\s+:\s+(\w+)";

            var output = RunShellCommand("netsh", "wlan show hostednetwork");

            var physicalAdresses = new List<PhysicalAddress>();
            var status = Regex.Match(output, string.Format(linePattern, statusKey));
            var clients = Regex.Match(output, string.Format(linePattern, numnerOfClientsKey));
            if (status.Groups[1].Value.Equals("Started") && !clients.Groups[1].Value.Equals("0"))
            {
                Logger.Log($"Found {clients.Groups[1].Value} connected clients", GetType().Name);
                var macMatch = Regex.Matches(output, @"(?<MAC>\w+:\w+:\w+:\w+:\w+:\w+)\s+Authenticated");
                foreach (Match match in macMatch)
                {
                    var adress = match.Groups["MAC"].Value.ToUpper().Replace(":", "-");
                    Logger.Log("FOUND MAC!! : " + adress, GetType().Name);
                    physicalAdresses.Add(PhysicalAddress.Parse(adress));
                }
            }

            return physicalAdresses;
        }

        private string RunShellCommand(string command, string args)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            });

            proc.WaitForExit();

            var output = proc.StandardOutput.ReadToEnd();
            return output;
        }

        public void Dispose()
        {
            hostedNetwork.Dispose();
        }
    }
}