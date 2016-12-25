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
using ControlApplication.Core;
using ControlApplication.DesktopClient.Controls;
using GMap.NET;

namespace ControlApplication.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize map:
            this.GMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            this.GMapControl.SetPositionByKeywords("Israel, Jerusalem");
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string address = this.TxtSearch.Text;
            this.GMapControl.SetPositionByKeywords(address);
        }

        private void PopAddDetectionWindow(object sender, MouseButtonEventArgs e)
        {
            new Window
            {
                Title = "Add new detection",
                Content = new AddDetectionControl(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }


        private void PopShowMarkerDetections(object sender, MouseButtonEventArgs e)
        {
            Detection[] detections =
            {
                new Detection(DateTime.Now, new Material("Coca", MaterialType.Explosive), new PointLatLng(), "3027744552", "36-019-19","33"),
                new Detection(DateTime.Now, new Material("Acitone", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Heroin", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Weed", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Brown", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("MD", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Vodka", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Toxicankjgfjhgfjhgfjhgf", MaterialType.Explosive), new PointLatLng(), "11", "22","33"),
                new Detection(DateTime.Now, new Material("Mashroom", MaterialType.Explosive), new PointLatLng(), "11", "22","33")
            };

            new Window
            {
                Title = "Show marker's detections",
                Content = new ShowMarkerDetections(detections),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}

