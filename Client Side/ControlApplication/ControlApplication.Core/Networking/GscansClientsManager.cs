﻿using System;
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
    public class GscansClientsManager : IGscanClientsApi
    {
        private WebClient _webClient;

        public GscansClientsManager()
        {
            _webClient = new WebClient();
        }

        public List<Detection> GetDeviceDetections(Tuple<PhysicalAddress, IPAddress> devices, Area activeArea)
        {
            var uri = new Uri($"http://{devices.Item2}:8080/getall");
            var result = _webClient.DownloadString(uri);

            var detectionKey = "detectionData";
            var ramanPathKey = "ramanPath";

            var pattern = $"(?<{detectionKey}>{{.*?}})\n<a href=\"(?<{ramanPathKey}>/\\w+)\">.+?</a>";
            var matches = Regex.Matches(result, pattern);

            var detectionsList = new List<Detection>();
            foreach (Match match in matches)
            {
                //Console.WriteLine($"{ramanPathKey}: {match.Groups[ramanPathKey]}");
                dynamic detectionObj =  JsonConvert.DeserializeObject(match.Groups[detectionKey].Value); //TODO: JSON can't convert

                DateTime dateTime = new DateTime(long.Parse(detectionObj.scan_time.ToString())); //TODO: Fix..
                string gscanSn = NetworkClientsFactory.GetNtServer().GetGscan(devices.Item1.ToString()).FirstOrDefault();
                string ramanOutput = "SHOULD HAVE RAMAN";//GetRaman(obj.raman_output_id.ToString()); //TODO: Get actual link
                var material = NetworkClientsFactory.GetNtServer().GetMaterial(name: detectionObj.material_detected.ToString());
                var detection = new Detection(dateTime, material[0], activeArea.RootLocation, activeArea, detectionObj.id_num.ToString(), detectionObj.plate_num.ToString(), gscanSn, ramanOutput);

                detectionsList.Add(detection);
            }
            
            return detectionsList;
        }

        public List<Tuple<PhysicalAddress, IPAddress>> GetConnectedDevices()
        {
            var devicesMacs = GetHostedNetworkConnectedDevices();
            var devicesIps = GetIpsFromPhisicalAdresses(devicesMacs);

            return devicesMacs.Select((t, i) => new Tuple<PhysicalAddress, IPAddress>(t, devicesIps[i])).ToList();
        }

        private List<IPAddress> GetIpsFromPhisicalAdresses(List<PhysicalAddress> physicalAddresses)
        {
            var ipPattern = @"(\d+\.\d+\.\d+\.\d+)\s+{0}";
            var output = RunShellCommand("arp", "-a");

            var ipAdresses = new List<IPAddress>();

            foreach (var physicalAddress in physicalAddresses)
            {
                var mac = Regex.Replace(physicalAddress.ToString(), ".{2}", "$0-").Remove(17);
                Console.WriteLine($"Getting ip for mac: {physicalAddress}");
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
                Console.WriteLine($"Found {clients.Groups[1].Value} connected clients");
                var macMatch = Regex.Matches(output, @"(?<MAC>\w+:\w+:\w+:\w+:\w+:\w+)\s+Authenticated");
                foreach (Match match in macMatch)
                {
                    var adress = match.Groups["MAC"].Value.ToUpper().Replace(":", "-");
                    Console.WriteLine("FOUND MAC!! : " + adress);
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
                RedirectStandardOutput = true
            });

            proc.WaitForExit();

            var output = proc.StandardOutput.ReadToEnd();
            return output;
        }
    }
}