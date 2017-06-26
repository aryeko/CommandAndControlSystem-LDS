using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        /// Filter for the current displayed detections
        /// </summary>
        internal MaterialType DetectionsFilter { get; set; }

        internal int NumberOfMaterialsToShow { get; set; }

        private PollingManager PollingManager { get; set; }

        private Area mWorkingArea;

        private readonly IMarkerableVisitor mMarkersVisitor;

        public Queue<CombinationAlertArgs> AlertsQueue { get; set; }

        internal Area ActiveMWorkingArea
        {
            get
            {
                return mWorkingArea;
            }
            set
            {
                mWorkingArea = value;
                LblWorkingArea.Content = $"Active area: {value.AreaType}";
                LblWorkingArea.Foreground = Brushes.Green;
            }
        }

        public MainWindow()
        {
            Login fLogin = new Login();
            fLogin.ShowDialog();
            
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;
            AlertsManager.CombinationFoundAlert += OnCombinationAlert;

            DetectionsFilter = ~MaterialType.None;
            AlertsQueue = new Queue<CombinationAlertArgs>();
            PollingManager = new PollingManager();
            WindowState = WindowState.Maximized;
            mMarkersVisitor = new MarkersVisitor(GMapControl);
        }

        private void OnCombinationAlert(object source, CombinationAlertArgs args)
        {
            Logger.Log("Alert was caught: " + args.AlertName, "MainWindow.AlertHandler");
            Networking.GetNtServer().AddAlert(new Alert(args.AlertName, args.Area, args.Detections, DateTime.Now));
            AlertsQueue.Enqueue(args);

            Task.Run(() =>
            {
                Logger.Log("Alert button is starting alarm", GetType().Name);
                bool dummyFlag = false;
                while (!args.Handled)
                {
                    Application.Current.Dispatcher.Invoke(
                        () => this.AlertsBtn.Background = dummyFlag ? Brushes.Red : Brushes.GhostWhite);
                    dummyFlag = !dummyFlag;
                    Thread.Sleep(500);
                }
                Logger.Log("Alert button is stopping the alarm", GetType().Name);
                Application.Current.Dispatcher.Invoke(
                    () => this.AlertsBtn.Background = Brushes.CornflowerBlue);
            });
        }

        internal async void LoadData(bool checkAlerts = true)
        {
            CircularProgressBar.Visibility = Visibility.Visible;

            Logger.Log("Starting load data", GetType().Name);
            if (!(checkAlerts && AlertsQueue.Any()))
            {
                var areas = await Task.Run(() => Networking.GetNtServer().GetArea());
                Logger.Log("Adding areas started", GetType().Name);
                foreach (var area in areas)
                {
                    AddMarker(area.RootLocation, area);
                }
                Logger.Log("Adding areas ended", GetType().Name);

                var exsistingDetections =
                    GMapControl.Markers
                    .Where(m => m.Shape is DetectionMarker)
                        .SelectMany(m => (m.Shape as DetectionMarker).GetAreaDetections()).ToList();
                Logger.Log($"Currently map has {exsistingDetections.Count()} detections", GetType().Name);

                var detections = await Task.Run(() => Networking.GetNtServer().GetDetections()
                                                        .Except(exsistingDetections)
                                                        .Where(d => DetectionsFilter.HasFlag(d.Material.MaterialType))
                                                        .GroupBy(d => d.Position));
                

                Logger.Log($"Adding detections started with {detections.Count()} new detections", GetType().Name);
                foreach (var detection in detections)
                {
                    AddMarker(detection.Key, detection.ToList());
                }
                Logger.Log("Adding detections ended", GetType().Name);

                Logger.Log("Load data ended", GetType().Name);
            }
            else
            {
                Logger.Log("Skipping load data", GetType().Name);
            }

            CircularProgressBar.Visibility = Visibility.Hidden;
        }

        
        internal void LoadDataFromClients()
        {
            if (!WirelessManagementControl.IsWifiEnabled)
            {
                Logger.Log("SKIPPING pull data from gscans - Wifi is not enabled", this.GetType().Name);
                return;
            }
            if (ActiveMWorkingArea == null)
            {
                Logger.Log("SKIPPING pull data from gscans - the active working area is not set", this.GetType().Name);
                return;
            }

            Logger.Log("Starting to pull data from gscans", this.GetType().Name);
            Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() => CircularProgressBar.Visibility = Visibility.Visible);

                var clients = Networking.GetGscanClientsApi().GetConnectedDevices();
                Logger.Log($"found {clients.Count} connected gscans", this.GetType().Name);

                foreach (var client in clients)
                {
                    var deviceDetections = Networking.GetGscanClientsApi().GetDeviceDetections(client, ActiveMWorkingArea);
                    var areaDetections = Networking.GetNtServer().GetDetections(areaId: ActiveMWorkingArea.DatabaseId);
                    var detectionsToAdd = deviceDetections.Except(areaDetections);
                    Logger.Log($"found {deviceDetections.Count} detections in gscan with ip {client.IpAddress}", this.GetType().Name);
                    Logger.Log($"area {ActiveMWorkingArea.AreaType} containes {areaDetections.Count} detections", this.GetType().Name);
                    Logger.Log($"adding {detectionsToAdd.Count()} new detections", this.GetType().Name);

                    foreach (var deviceDetection in detectionsToAdd)
                    {
                        Networking.GetNtServer().AddDetection(deviceDetection);
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

            LblWorkingArea.Content = "THE ACTIVE WORKING AREA IS NOT SET";
            LblWorkingArea.Foreground = Brushes.Red;
            Networking.GetNtServer().CacheCrucialObjects();
            LoadData(false);
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomControl.UpdateControl();
        }

        private void PopAddDetectionWindow(object sender, MouseButtonEventArgs e)
        {
            if (ActiveMWorkingArea == null)
            {
                MessageBox.Show(this, "Please set the active working area", "Please set the active working area",
                    MessageBoxButton.OK);
                return;
            }
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
                markerable.Accept(mMarkersVisitor);
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
            Networking.GetGscanClientsApi().TryStopHostedNetwork();

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
            var materials = Networking.GetNtServer().GetMaterial().OrderBy(m => m.Name).ToList();
            
            new Window
            {
                Title = "Materials list from DB",
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

        private void AlertsBtn_Click(object sender, RoutedEventArgs e)
        {
            var alertList = Networking.GetNtServer().GetAlerts().OrderBy(a => a.AlertTime.Millisecond).Reverse().ToList();
            new Window
            {
                Title = "Show alerts list",
                Content = new ShowAlertsControl(alertList),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void WirelessBtn_Click(object sender, RoutedEventArgs e)
        {
            new Window
            {
                Title = "Wirless Management",
                Content = new WirelessManagementControl(),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}

