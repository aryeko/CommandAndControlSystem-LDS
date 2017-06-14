using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for SingleDeviceControl.xaml
    /// </summary>
    public partial class SingleDeviceControl : UserControl
    {
        public Gscan Gscan { get; }

        private bool _isAvilable = true;
        public bool IsAvilable
        {
            get { return _isAvilable; }
            set
            {
                _isAvilable = value;
                StatusIcon.Source = new BitmapImage(new Uri($"../Drawable/device_status_{(value ? "green" : "red")}.png", UriKind.Relative));          
            }
        }

        public SingleDeviceControl(Gscan gscan)
        {
            Gscan = gscan;
            InitializeComponent();

            LblMac.Content = gscan.PhysicalAddress;
            LblIp.Content = gscan.IpAddress;
        }
    }
}
