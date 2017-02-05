using System;
using System.IO;
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
using GMap.NET;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal const int MARKER_SIZE = 50;

        /// <summary>
        /// HostedNetwork API
        /// </summary>
        private readonly HostedNetwork _hostedNetwork;

        public MainWindow()
        {
            this._hostedNetwork = new HostedNetwork();
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            MouseWheel += MainWindow_MouseWheel;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize map:
            this.GMapControl.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            this.GMapControl.SetPositionByKeywords("Israel, Jerusalem");

            ZoomControl.UpdateControl();

            _hostedNetwork.StartHostedNetwork();
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

        internal void AddMarker(Point p)
        {
            GMapMarker marker =
                new GMapMarker(GMapControl.FromLocalToLatLng((int) p.X - MARKER_SIZE / 2, (int) p.Y - MARKER_SIZE))
                {
                    Shape = GetImage(new Uri(@"\Drawable\MapMarker_Blue.png", UriKind.Relative))
                };

            GMapControl.Markers.Add(marker);
        }

        private Image GetImage(Uri uri)
        {
            Image myImage = new Image();
            myImage.Width = MARKER_SIZE;

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
    }
}

