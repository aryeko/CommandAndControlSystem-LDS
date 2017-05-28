using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
using CheckBox = System.Windows.Controls.CheckBox;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ChooseDetectionType.xaml
    /// </summary>
    public partial class ChooseDetectionType : UserControl
    {
        public ChooseDetectionType()
        {
            InitializeComponent();

            var j = 0;
            foreach (var material in Enum.GetValues(typeof(MaterialType)))
            {
                DetectionDataXmal.RowDefinitions.Insert(j, new RowDefinition());
                var cb = new CheckBox
                {
                    Name = material.ToString(),
                    Content = material.ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                };

                Grid.SetRow(cb, j);
                DetectionDataXmal.Children.Add(cb);

                j++;   
            }
        }

        /// <summary>
        /// Gets the MainWindow control
        /// </summary>
        /// <returns>The MainWindow or null on error</returns>
        private static MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
            var cbValues = (from CheckBox cb in DetectionDataXmal.Children where cb.IsChecked != null && cb.IsChecked.Value select cb.Name).ToList();

            if (cbValues.Count == 0)
                return;
            
            GetMainWindow().DeleteMarkers();
            ShowSelectedDetections(cbValues);
        }

        private void ShowSelectedDetections(List<string> cbValues)
        { 
            var mainWindow = GetMainWindow();
            Task.Run(() =>
            {
                mainWindow.Dispatcher.Invoke(() => mainWindow.CircularProgressBar.Visibility = Visibility.Visible);
                var detectionsToShow = GetMainWindow().RemoteServerApi.GetDetections().Where(d => d.Material.IsContainsMaterialType(cbValues));

                foreach (var detection in detectionsToShow)
                    mainWindow.Dispatcher.Invoke(()=> mainWindow.AddMarker(detection.Position, new List<Detection> { detection }));

                mainWindow.Dispatcher.Invoke(() => mainWindow.CircularProgressBar.Visibility = Visibility.Hidden);
            });
        }
           
    }
}
