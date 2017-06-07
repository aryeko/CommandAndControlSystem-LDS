using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using ControlApplication.Core;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using ControlApplication.DesktopClient.Controls;
using ControlApplication.DesktopClient.Controls.Markers;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMapMarker = GMap.NET.WindowsPresentation.GMapMarker;


namespace ControlApplication.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal const int MARKER_SIZE = 25;

        /// <summary>
        /// 
        /// </summary>
        internal MaterialType DetectionsFilter { get; set; }

        internal int NumberOfMaterialsToShow { get; set; }

        /// <summary>
        /// HostedNetwork API
        /// </summary>
        private readonly HostedNetwork _hostedNetwork;

        private PollingManager PollingManager { get; set; }

        internal Area ActiveWorkingArea { get; set; }

        public MainWindow()
        {
            Login fLogin = new Login();
            fLogin.ShowDialog();
            DetectionsFilter = ~MaterialType.None;
            ActiveWorkingArea = new Area(new PointLatLng(0,0), AreaType.Undefined, 0);

            this._hostedNetwork = new HostedNetwork();
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;
            PollingManager = new PollingManager();
            WindowState = WindowState.Maximized;
        }

        internal void LoadData()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => CircularProgressBar.Visibility = Visibility.Visible);
                var detections = NetworkClientsFactory.GetNtServer().GetDetections()
                .Where(d => DetectionsFilter.HasFlag(d.Material.MaterialType))
                .GroupBy(d => d.Position);
                var areas = NetworkClientsFactory.GetNtServer().GetArea();
                foreach (var detection in detections)
                {
                    Application.Current.Dispatcher.Invoke(() => AddMarker(detection.Key, detection.ToList()));
                }
                foreach (var area in areas)
                {
                    Application.Current.Dispatcher.Invoke(() => AddMarker(area.RootLocation, area));
                }
                Application.Current.Dispatcher.Invoke(() => CircularProgressBar.Visibility = Visibility.Hidden);
            });
        }

        internal void LoadDataFromClients()
        {
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => CircularProgressBar.Visibility = Visibility.Visible);
                var gscansManager = new GscansClientsManager();
                var clients = gscansManager.GetConnectedDevices();
                foreach (var client in clients)
                {
                    var cliendDetections = gscansManager.GetDeviceDetections(client, ActiveWorkingArea).GroupBy(d => d.Position);
                    foreach (var cliendDetection in cliendDetections)
                    {
                        Application.Current.Dispatcher.Invoke(() => AddMarker(cliendDetection.Key, cliendDetection));
                    }
                }
                Application.Current.Dispatcher.Invoke(() => CircularProgressBar.Visibility = Visibility.Hidden);
            });
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize map:
            this.GMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            this.GMapControl.SetPositionByKeywords("Israel, Jerusalem");
            ZoomControl.UpdateControl();
            
            LoadData();

            //TODO: Start Hosted network with a button (handle the case when a client don't support Hosted network 
            //  _hostedNetwork.StartHostedNetwork();
        }
        
        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomControl.UpdateControl();
        }

        private void PopAddDetectionWindow(object sender, MouseButtonEventArgs e)
        {
            new Window
            {
                Title = "Add new detection",
                Content = new AddDetectionControl(new Point(e.GetPosition(this).X, e.GetPosition(this).Y)),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        internal void AddMarker(Point p, IEnumerable<IMarkerable> markerables)
        {
            AddMarker(GMapControl.FromLocalToLatLng((int) p.X, (int) p.Y), markerables);
        }

        internal void AddMarker(PointLatLng p, IMarkerable markerable)
        {
            AddMarker(p, new []{markerable});
        }

        internal void AddMarker(PointLatLng p, IEnumerable<IMarkerable> markerables)
        {
            foreach (var markerable in markerables)
            {
                markerable.Accept(new MarkersManager(GMapControl));
            }
        }

        internal void DeleteMarkers()
        {
            GMapControl.Markers.Clear();
        }

        private Image GetImage(Uri uri)
        {
            Image myImage = new Image {Width = MARKER_SIZE};

            // Create source
            BitmapImage myBitmapImage = new BitmapImage();

            // BitmapImage.UriSource must be in a BeginInit/EndInit block
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = uri;

            // To save significant application memory, set the DecodePixelWidth or   
            // DecodePixelHeight of the BitmapImage value of the image source to the desired  
            // height or width of the rendered image. If you don't do this, the application will  
            // cache the image as though it were rendered as its normal size rather then just  
            // the size that is displayed. 
            // Note: In order to preserve aspect ratio, set DecodePixelWidth 
            // or DecodePixelHeight but not both.
            myBitmapImage.DecodePixelWidth = MARKER_SIZE;
            myBitmapImage.EndInit();
            //set image source
            myImage.Source = myBitmapImage;

            return myImage;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DetectionBtn_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            new Window
            {
                Title = "Choose detections",
                Content = new ChooseDetectionType(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void AddPointBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Equals(AddPointBtn.Background, Brushes.CornflowerBlue))
            {
                GMapControl.MouseDoubleClick += PopAddDetectionWindow;
                AddPointBtn.Background = Brushes.DarkBlue;
                AddPointBtn.ToolTip = "Double click on the map to add a manual detection";
            }
            else
            {
                GMapControl.MouseDoubleClick -= PopAddDetectionWindow;
                AddPointBtn.Background = Brushes.CornflowerBlue;
                AddPointBtn.ToolTip = "Press the button to add a manual detection";
            }

            if (Equals(AddAreaBtn.Background, Brushes.DarkBlue))
            {
                GMapControl.MouseDoubleClick -= PopAddAreaWindow;
                AddAreaBtn.Background = Brushes.CornflowerBlue;
                AddAreaBtn.ToolTip = "Press the button to add an area";
            }
        }

        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            var materials = NetworkClientsFactory.GetNtServer().GetMaterial().OrderBy(m => m.Name).ToList();
            
            new Window
            {
                Title = "Show Materials",
                Content = new MaterialsList(materials),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void AddAreaBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Equals(AddAreaBtn.Background, Brushes.CornflowerBlue))
            {
                GMapControl.MouseDoubleClick += PopAddAreaWindow;
                AddAreaBtn.Background = Brushes.DarkBlue;
                AddAreaBtn.ToolTip = "Double click on the map to add an area";
            }
            else
            {
                GMapControl.MouseDoubleClick -= PopAddAreaWindow;
                AddAreaBtn.Background = Brushes.CornflowerBlue;
                AddAreaBtn.ToolTip = "Press the button to add aan area";
            }

            if (Equals(AddPointBtn.Background, Brushes.DarkBlue))
            {
                GMapControl.MouseDoubleClick -= PopAddDetectionWindow;
                AddPointBtn.Background = Brushes.CornflowerBlue;
                AddPointBtn.ToolTip = "Press the button to add a manual detection";
            }
        }

        private void PopAddAreaWindow(object sender, MouseButtonEventArgs e)
        {
            new Window
            {
                Title = "Add new area",
                Content = new AddAreaControl(new Point(e.GetPosition(this).X, e.GetPosition(this).Y)),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void OptionBtn_OnClick(object sender, RoutedEventArgs e)
        {
            NumberOfMaterialsToShow = 2;
            new Window
            {
                Title = "Choose an option",
                Content = new OptionControl(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}

