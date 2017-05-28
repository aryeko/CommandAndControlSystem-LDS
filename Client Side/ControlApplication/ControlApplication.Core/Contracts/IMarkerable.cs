using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// Interface for markerable elements
    /// </summary>
    public interface IMarkerable
    {
        void Accept(IMarkerableVisitor visitor);
    }
}
