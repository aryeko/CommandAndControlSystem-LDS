using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// 
        /// </summary>
        internal MaterialType DetectionsFilter { get; set; }

        internal int NumberOfMaterialsToShow { get; set; }

        /// <summary>
        /// HostedNetwork API
        /// </summary>
        private readonly HostedNetwork _hostedNetwork;

        private PollingManager PollingManager { get; set; }

        private Area _workingArea;

        public Queue<CombinationAlertArgs> AlertsQueue { get; set; }

        internal Area ActiveWorkingArea
        {
            get
            {
                return _workingArea;
            }
            set
            {
                _workingArea = value;
                LblWorkingArea.Content = $"Active area: {value.AreaType}";
                LblWorkingArea.Foreground = Brushes.Green;
            }
        }

        public MainWindow()
        {
            Login fLogin = new Login();
            fLogin.ShowDialog();
            DetectionsFilter = ~MaterialType.None;
            AlertsQueue = new Queue<CombinationAlertArgs>();
            this._hostedNetwork = new HostedNetwork();
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;
            AlertsManager.CombinationFoundAlert += OnCombinationAlert;
            PollingManager = new PollingManager();
            WindowState = WindowState.Maximized;
        }

        private void OnCombinationAlert(object source, CombinationAlertArgs args)
        {
            AlertsQueue.Enqueue(args);
            AlertsBtn.Background = Brushes.Red;
            Task.Run(() =>
            {
                Logger.Log($"[{DateTime.Now.TimeOfDay.ToString("g")}] Alert button is starting alarm", GetType().Name);
                bool dummyFlag = false;
                while (!args.Handled)
                {
                    Application.Current.Dispatcher.Invoke(
                        () => this.AlertsBtn.Background = dummyFlag ? Brushes.Red : Brushes.GhostWhite);
                    dummyFlag = !dummyFlag;
                    Thread.Sleep(500);
                }
                Logger.Log($"[{DateTime.Now.TimeOfDay.ToString("g")}] Alert button is stopping the alarm", GetType().Name);
                Application.Current.Dispatcher.Invoke(
                    () => this.AlertsBtn.Background = Brushes.CornflowerBlue);
            });
        }

        internal void LoadData(bool checkAlerts = true)
        {
            Task.Run(() =>
            {
                Logger.Log("Starting load data", GetType().Name);
                if (checkAlerts && AlertsQueue.Any())
                {
                    Logger.Log("Skipping load data", GetType().Name);
                    return;
                }

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => CircularProgressBar.Visibility = Visibility.Visible));
                var detections = NetworkClientsFactory.GetNtServer().GetDetections()
                .Where(d => DetectionsFilter.HasFlag(d.Material.MaterialType))
                .GroupBy(d => d.Position);
                var areas = NetworkClientsFactory.GetNtServer().GetArea();
                foreach (var detection in detections)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => AddMarker(detection.Key, detection.ToList())));
                }
                foreach (var area in areas)
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => AddMarker(area.RootLocation, area)));
                }
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => CircularProgressBar.Visibility = Visibility.Hidden));
                Logger.Log("Load data ended", GetType().Name);
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

            LblWorkingArea.Content = "THE ACTIVE WORKING AREA IS NOT SET";
            LblWorkingArea.Foreground = Brushes.Red;
			//CacheMaterials();
            LoadData(false);

            //TODO: Start Hosted network with a button (handle the case when a client don't support Hosted network 
            //  _hostedNetwork.StartHostedNetwork();
        }

        //private void CacheMaterials()
        //{
        //    dynamic materials = NetworkClientsFactory.GetNtServer(false).GetObject("material");

        //    foreach (dynamic material in materials)
        //    {
        //        Console.WriteLine("Name is: " + material.name.ToString() + " ID is: " + material._id.ToString());
        //        NetworkClientsFactory.GetNtServer().SetObject(material.name.ToString(), material);
        //        NetworkClientsFactory.GetNtServer().SetObject(material._id.ToString(), material);
        //    }
        //}

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomControl.UpdateControl();
        }

        private void PopAddDetectionWindow(object sender, MouseButtonEventArgs e)
        {
            if (ActiveWorkingArea == null)
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
            if(AlertsQueue.Any())
                AlertsQueue.Dequeue().Handled = true;

            List<Detection> detections = NetworkClientsFactory.GetNtServer().GetDetections();
            Area area = NetworkClientsFactory.GetNtServer().GetArea().First();
            Alert alert = new Alert("Test", area, detections,DateTime.Now);
            List<Alert> alertList = new List<Alert> { alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert, alert,alert, alert,alert,alert, alert, alert, alert, alert , alert, alert, alert, alert , alert, alert, alert, alert}; //NetworkClientsFactory.GetNtServer().GetAlerts();
            new Window
            {
                Title = "Show alerts list",
                Content = new ShowAlertsControl(alertList),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}

