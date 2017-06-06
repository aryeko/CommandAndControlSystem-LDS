using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
