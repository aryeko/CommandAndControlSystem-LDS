using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApplication.Core.Contracts;

namespace ControlApplication.Core.Networking
{
    public class DetectionAddedEventArgs : EventArgs
    {
        public DetectionAddedEventArgs(Detection detection)
        {
            Detection = detection;
        }

        public Detection Detection { get; }      
    }
}
