using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ControlApplication.Core;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient.Controls.Markers
{
    /// <summary>
    /// Interaction logic for AreaMarker.xaml
    /// </summary>
    public partial class AreaMarker : UserControl
    {
        private GMapMarker mMarker;
        private Area mArea;
        private Popup mPopup;
        private Label mLabel;

        public AreaMarker(GMapMarker marker, Area area)
        {
            InitializeComponent();

            this.mMarker = marker;
            this.mArea = area;

            this.MouseEnter += MarkerMouseEnter;
            this.MouseLeave += MarkerMouseLeave;
            this.MouseRightButtonDown += MarkerRightMouseDown;
            this.MouseRightButtonUp += MarkerRightMouseUp;
            this.PreviewMouseDoubleClick += MarkerDoubleClicked;

            mPopup = new Popup { Placement = PlacementMode.Mouse };
            mLabel = new Label
            {
                Background = Brushes.Blue,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.WhiteSmoke,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(5),
                FontSize = 22,
                Content = $"{mArea.AreaType}"
            };
            mPopup.Child = mLabel;

            AlertsManager.CombinationFoundAlert += OnCombinationAlert;
        }

        private void OnCombinationAlert(object source, CombinationAlertArgs args)
        {
            Console.WriteLine($"Area {mArea.AreaType} at {mArea.RootLocation} being notified for combination alert");
            if (!args.Area.Equals(mArea))
                return;
            Console.WriteLine($"Area {mArea.AreaType} at {mArea.RootLocation} is responding to combination alert");

            (Application.Current.MainWindow as MainWindow).GMapControl.Position = mArea.RootLocation;
            (Application.Current.MainWindow as MainWindow).GMapControl.Zoom = 15;
            Task.Run(() =>
            {
                Console.WriteLine($"Area {mArea.AreaType} is starting alarm");
                bool dummyFlag = false;
                while (!args.Handled)
                {
                    Application.Current.Dispatcher.Invoke(
                        () => this.markerIcon.Source = new BitmapImage(new Uri($"/ControlApplication.DesktopClient;component/Drawable/MapMarker_{(dummyFlag ? "Red" : "Area")}.png", UriKind.Relative)));
                    dummyFlag = !dummyFlag;
                    Thread.Sleep(500);
                }
                Console.WriteLine($"Area {mArea.AreaType} is stopping the alarm");
                Application.Current.Dispatcher.Invoke(
                    () => this.markerIcon.Source = new BitmapImage(new Uri($"/ControlApplication.DesktopClient;component/Drawable/MapMarker_Area.png", UriKind.Relative)));
            });
        }

        private void MarkerDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var sMessageBoxText = $"Do you want to this {mArea.AreaType} area as the working area?";
            var sCaption = "Laser Detect Systems - NT";
            
            var messageBoxResult = MessageBox.Show(sMessageBoxText, sCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            switch (messageBoxResult)
            {
                case MessageBoxResult.Yes:
                    Console.WriteLine($"Set {mArea.AreaType} at {mArea.RootLocation} as the active working area");
                    (Application.Current.MainWindow as MainWindow).ActiveWorkingArea = mArea;
                    break;

                case MessageBoxResult.No:
                    Console.WriteLine($"[CANCELED] Set {mArea.AreaType} at {mArea.RootLocation} as the active working area");
                    break;

                default:
                    Console.WriteLine($"Area marker clicked with not supported option: {messageBoxResult}");
                    break;
            }

        }

        private void MarkerRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured) return;
            Mouse.Capture(null);
            
            new Window
            {
                Title = "Show Area Details",
                Icon = new BitmapImage(new Uri(@"../../Drawable/logo.png", UriKind.Relative)),
                //Icon = new BitmapImage(new Uri(@"/ControlApplication.DesktopClient;/Drawable/logo.png", UriKind.Absolute)),
                Content = new ShowAreaDetails(mArea, NetworkClientsFactory.GetNtServer().GetDetections().Where(d => d.Area.Equals(mArea))),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
            
        }

        private void MarkerRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured) return;

            Mouse.Capture(this);
        }
        
        private void MarkerMouseLeave(object sender, MouseEventArgs e)
        {
            mMarker.ZIndex -= 10000;
            mPopup.IsOpen = false;
        }

        private void MarkerMouseEnter(object sender, MouseEventArgs e)
        {
            mMarker.ZIndex += 10000;
            mPopup.IsOpen = true;
        }
    }
}
