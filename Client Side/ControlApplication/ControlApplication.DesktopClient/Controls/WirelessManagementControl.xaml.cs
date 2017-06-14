using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for WirelessManagementControl.xaml
    /// </summary>
    public partial class WirelessManagementControl : UserControl
    {
        private static bool IsWifiEnabled { get; set; } = false;

        public WirelessManagementControl()
        {
            InitializeComponent();
            Loaded += OnLoad;
            TxtWifiSsid.Visibility = Visibility.Hidden;
            ButtonRefresh.Visibility = Visibility.Hidden;  
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            CheckBoxWifiStatus.IsChecked = IsWifiEnabled;
            if(IsWifiEnabled)
                CheckBoxWifiStatus_Checked(this, null);

            Window window = Window.GetWindow(this);
            window.Closing += OnClose;
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            IsWifiEnabled = CheckBoxWifiStatus.IsChecked.Value;
        }

        private void CheckBoxWifiStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (!NetworkClientsFactory.GetGscanClientsApi().TryStartHostedNetwork())
            {
                MessageBox.Show(Application.Current.MainWindow, "Hosted Network Error" ,"Hosted network is not supported in your computer", MessageBoxButton.OK);
                CheckBoxWifiStatus.IsChecked = false;
                return;
            }

            TxtWifiSsid.Text = NetworkClientsFactory.GetGscanClientsApi().GetHostedNetwordSsid();
            TxtWifiSsid.Visibility = Visibility.Visible;
            ButtonRefresh.Visibility = Visibility.Visible;
            UpdateConnectedDevices();
        }

        private void UpdateConnectedDevices()
        {
            if (!CheckBoxWifiStatus.IsChecked.Value)
                return;

            var connectedDevices = NetworkClientsFactory.GetGscanClientsApi().GetConnectedDevices();

            foreach (SingleDeviceControl control in DevicesListScroll.Children)
            {
                control.IsAvilable = connectedDevices.Contains(control.Gscan);
            }

            foreach (var device in connectedDevices)
            {
                if (DevicesListScroll.Children.Cast<SingleDeviceControl>().Any(child => child.Gscan.Equals(device)))
                    continue;
                
                var newRowIndex = DevicesListScroll.RowDefinitions.Count;
                DevicesListScroll.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                var newControl = new SingleDeviceControl(device);

                Grid.SetRow(newControl, newRowIndex);
                DevicesListScroll.Children.Add(newControl);
            }
        }

        private void CheckBoxWifiStatus_UnChecked(object sender, RoutedEventArgs e)
        {
            NetworkClientsFactory.GetGscanClientsApi().TryStopHostedNetwork();
            TxtWifiSsid.Visibility = Visibility.Hidden;

            DevicesListScroll.Children.Clear();
        }

        private void ButtonRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateConnectedDevices();
        }
    }
}
