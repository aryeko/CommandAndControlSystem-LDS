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
    /// Interaction logic for SingleAlertControl.xaml
    /// </summary>
    public partial class SingleAlertControl : UserControl
    {
        private readonly Alert _alert;

        public SingleAlertControl(Alert alert)
        {
            InitializeComponent();

            LblAlertName.Text = alert.AlertName;
            Border.BorderBrush = !alert.IsDirty ? Brushes.Red : Brushes.White;
            LblDateTime.Text = alert.AlertTime.ToString("G");
            _alert = alert;
        }
        
        private void LblAlertName_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_alert.IsDirty)
            {
                _alert.IsDirty = true;
                NetworkClientsFactory.GetNtServer().UpdateAlert(_alert);
                Border.BorderBrush = Brushes.White;
                if ((Application.Current.MainWindow as MainWindow).AlertsQueue.Any())
                    (Application.Current.MainWindow as MainWindow).AlertsQueue.Dequeue().Handled = true;
            }
            new Window
            {
                Title = "Showing all alert's detections",
                Content = new ShowMarkerDetections(_alert.Detections),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}
