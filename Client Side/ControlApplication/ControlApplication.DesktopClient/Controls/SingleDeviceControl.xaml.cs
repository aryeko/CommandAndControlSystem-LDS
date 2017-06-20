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
using ControlApplication.Core.Networking;

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
            MouseDoubleClick += OnDoubleClick;
            MouseRightButtonUp += OnRightMouseUp;
        }

        private void OnRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            var activeArea = (Application.Current.MainWindow as MainWindow).ActiveMWorkingArea;
            if (activeArea == null)
            {
                MessageBox.Show(Window.GetWindow(this), "Please set the active working area", "Please set the active working area",
                    MessageBoxButton.OK);
                Window.GetWindow(this).Close();
                return;
            }

            var deviceDetections = NetworkClientsFactory.GetGscanClientsApi().GetDeviceDetections(Gscan, activeArea);
            var areaDetections = NetworkClientsFactory.GetNtServer().GetDetections(areaId: activeArea.DatabaseId);
            var detectionsToAdd = deviceDetections.Except(areaDetections);
            foreach (var deviceDetection in detectionsToAdd)
            {
                NetworkClientsFactory.GetNtServer().AddDetection(deviceDetection);
            }           
        }

        private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show(Window.GetWindow(this),
                "Are you sure you want to add this Gscan to the database?", "Add Gscan to the database",
                MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                NetworkClientsFactory.GetNtServer().AddGscan(Gscan);
            }
        }
    }
}
