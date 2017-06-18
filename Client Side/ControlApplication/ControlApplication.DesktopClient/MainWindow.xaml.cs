﻿using System;
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
            
            InitializeComponent();
            
            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;
            AlertsManager.CombinationFoundAlert += OnCombinationAlert;

            DetectionsFilter = ~MaterialType.None;
            AlertsQueue = new Queue<CombinationAlertArgs>();
            PollingManager = new PollingManager();
            WindowState = WindowState.Maximized;
        }

        private void OnCombinationAlert(object source, CombinationAlertArgs args)
        {
            Logger.Log("Alert was caught: " + args.AlertName, "MainWindow.AlertHandler");
            NetworkClientsFactory.GetNtServer().AddAlert(new Alert(args.AlertName, args.Area, args.Detections, DateTime.Now));
            AlertsQueue.Enqueue(args);
            AlertsBtn.Background = Brushes.Red;
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
                var areas = await Task.Run(() => NetworkClientsFactory.GetNtServer().GetArea());
                Logger.Log("Adding areas started", GetType().Name);
                foreach (var area in areas)
                {
                    AddMarker(area.RootLocation, area);
                }
                Logger.Log("Adding areas ended", GetType().Name);
                var detections = await Task.Run(() => NetworkClientsFactory.GetNtServer().GetDetections()
                                                        .Where(d => DetectionsFilter.HasFlag(d.Material.MaterialType))
                                                        .GroupBy(d => d.Position));
                Logger.Log("Adding detections started", GetType().Name);
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

        /*
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
        */
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize map:
            this.GMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            this.GMapControl.SetPositionByKeywords("Israel, Jerusalem");
            ZoomControl.UpdateControl();

            LblWorkingArea.Content = "THE ACTIVE WORKING AREA IS NOT SET";
            LblWorkingArea.Foreground = Brushes.Red;
            CacheCrucialObjects();
            LoadData(false);
        }

        private void CacheCrucialObjects()
        {
            Logger.Log("Caching Materials and Areas", "MainWindow.CacheCrucialObjects");

            dynamic materials = NetworkClientsFactory.GetNtServer(false).GetObject("material");

            foreach (dynamic material in materials)
            {
                NetworkClientsFactory.GetNtServer().SetObject(material.name.ToString(), material);
                NetworkClientsFactory.GetNtServer().SetObject(material._id.ToString(), material);
            }

            dynamic areas = NetworkClientsFactory.GetNtServer(false).GetObject("area");
            foreach (dynamic area in areas)
            {
                NetworkClientsFactory.GetNtServer().SetObject(area.root_location.ToString(), area);
                NetworkClientsFactory.GetNtServer().SetObject(area._id.ToString(), area);
            }

            dynamic detections = NetworkClientsFactory.GetNtServer(false).GetObject("detection");
            foreach (dynamic detection in detections)
            {
                NetworkClientsFactory.GetNtServer().SetObject(detection._id.ToString(), detection);
            }
        }

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
            NetworkClientsFactory.GetGscanClientsApi().TryStopHostedNetwork();

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

            var alertList = NetworkClientsFactory.GetNtServer().GetAlerts().OrderBy(a => a.AlertTime.TimeOfDay).Reverse().ToList();
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

