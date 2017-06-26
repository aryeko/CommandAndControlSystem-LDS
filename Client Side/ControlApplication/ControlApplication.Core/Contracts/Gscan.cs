using System;
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
        /// Gets the owned unit database ID
        /// </summary>
        public string OwnedUnitDatabaseId { get; }

        /// <summary>
        /// Gets/sets the last time that this Gscan device has been queried for data
        /// </summary>
        public string LastQueryTime { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="physicalAddress"></param>
        /// <param name="ipAddress"></param>
        /// <param name="ownedUnitDatabaseId"></param>
        public Gscan(PhysicalAddress physicalAddress, IPAddress ipAddress, string ownedUnitDatabaseId = "")
        {
            PhysicalAddress = physicalAddress;
            IpAddress = ipAddress;
            OwnedUnitDatabaseId = ownedUnitDatabaseId;
            LastQueryTime = "0";
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
