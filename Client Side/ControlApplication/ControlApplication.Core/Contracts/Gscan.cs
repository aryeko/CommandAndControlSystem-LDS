﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// This class is representing a connected Gscan device
    /// </summary>
    public class Gscan : IEquatable<Gscan>
    {
        /// <summary>
        /// Gets device's physical adress
        /// </summary>
        public PhysicalAddress PhysicalAddress { get; }

        /// <summary>
        /// Gets device's IP adress
        /// </summary>
        public IPAddress IpAddress { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="physicalAddress"></param>
        /// <param name="ipAddress"></param>
        public Gscan(PhysicalAddress physicalAddress, IPAddress ipAddress)
        {
            PhysicalAddress = physicalAddress;
            IpAddress = ipAddress;
        }

        public override int GetHashCode()
        {
            return PhysicalAddress.GetHashCode();
        }

        public bool Equals(Gscan other)
        {
            return other != null && PhysicalAddress.Equals(other.PhysicalAddress);
        }
    }
}
