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
            //this.markerIcon.Source = new BitmapImage(new Uri($"/ControlApplication.DesktopClient;component/Drawable/MapMarker_{GetBrush().Item1}.png", UriKind.Relative));
        }
        
        private void MarkerDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (!Equals((Application.Current.MainWindow as MainWindow).AddPointBtn.Background, Brushes.DarkBlue)) return;

            new Window
            {
                Title = "Append a new detection",
                Content = new AddDetectionControl(mMarker.Position),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void MarkerRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured) return;
            Mouse.Capture(null);
            /*
            new Window
            {
                Title = "Show mMarker's detections",
                Content = new ShowMarkerDetections(mDetections),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
            */
        }

        private void MarkerRightMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured) return;

            Mouse.Capture(this);
        }
        
        private void MarkerMouseLeave(object sender, MouseEventArgs e)
        {
            mMarker.ZIndex -= 10000;
        }

        private void MarkerMouseEnter(object sender, MouseEventArgs e)
        {
            mMarker.ZIndex += 10000;
        }
    }
}
