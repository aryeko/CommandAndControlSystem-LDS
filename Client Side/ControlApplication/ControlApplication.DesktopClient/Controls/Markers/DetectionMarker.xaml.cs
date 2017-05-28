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
    /// Interaction logic for DetectionMarker.xaml
    /// </summary>
    public partial class DetectionMarker : UserControl
    {
        private GMapMarker mMarker;
        private IEnumerable<Detection> mDetections;
        private Popup mPopup;
        private Label mLabel;

        public DetectionMarker(GMapMarker marker, Detection detection) 
            : this(marker, new []{detection})
        {       
        }

        public DetectionMarker(GMapMarker marker, IEnumerable<Detection> detections)
        {
            InitializeComponent();

            this.mMarker = marker;
            this.mDetections = detections;

            this.MouseEnter += MarkerMouseEnter;
            this.MouseLeave += MarkerMouseLeave;
            this.MouseRightButtonDown += MarkerRightMouseDown;
            this.MouseRightButtonUp += MarkerRightMouseUp;
            this.PreviewMouseDoubleClick += MarkerDoubleClicked;
            this.markerIcon.Source = new BitmapImage(new Uri($"/ControlApplication.DesktopClient;component/Drawable/MapMarker_{GetBrush().Item1}.png", UriKind.Relative));
            mPopup = new Popup {Placement = PlacementMode.Mouse};
            mLabel = new Label
            {
                Background = GetBrush().Item2,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.WhiteSmoke,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(5),
                FontSize = 22,
                Content = string.Join(" - ", mDetections.Select(d => d.Material.MaterialType).Distinct())
            };
            mPopup.Child = mLabel;
        }

        public void AddDetections(IEnumerable<Detection> detections)
        {
            mDetections = mDetections.Concat(detections).ToList();
            mLabel.Content = string.Join(" - ", mDetections.Select(d => d.Material.MaterialType).Distinct());
            mLabel.Background = GetBrush().Item2;
        }

        private Tuple<string,SolidColorBrush> GetBrush()
        {
            if(mDetections.Any(d => d.Material.MaterialType == MaterialType.Explosive))
                return new Tuple<string, SolidColorBrush>("Red", Brushes.Red);
            else if (mDetections.Any(d => d.Material.MaterialType == MaterialType.Narcotics))
                return new Tuple<string, SolidColorBrush>("Yellow", Brushes.Yellow);
            return new Tuple<string, SolidColorBrush>("Green", Brushes.Green);
        }

        private void MarkerDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (!Equals((Application.Current.MainWindow as MainWindow).AddPointBtn.Background, Brushes.DarkBlue)) return;

            new Window
            {
                Title = "Append a new detection",
                Content = new AddDetectionControl(mMarker.Position, mDetections),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void MarkerRightMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured) return;
            Mouse.Capture(null);

            new Window
            {
                Title = "Show mMarker's detections",
                Content = new ShowMarkerDetections(mDetections),
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
