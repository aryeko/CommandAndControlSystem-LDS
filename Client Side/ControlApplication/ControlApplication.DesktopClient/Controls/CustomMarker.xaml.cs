using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ControlApplication.Core.Contracts;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for CustomMarker.xaml
    /// </summary>
    public partial class CustomMarker : UserControl
    {
        private GMapMarker mMarker;
        private MainWindow mMainWindow;
        private IEnumerable<Detection> mDetections;
        private Popup mPopup;
        private Label mLabel;

        public CustomMarker(MainWindow mMainWindow, GMapMarker mMarker, IEnumerable<Detection> detections)
        {
            InitializeComponent();

            this.mMarker = mMarker;
            this.mDetections = detections;
            this.mMainWindow = mMainWindow;

            this.MouseEnter += MarkerMouseEnter;
            this.MouseLeave += MarkerMouseLeave;
            this.MouseRightButtonDown += MarkerRightMouseDown;
            this.MouseRightButtonUp += MarkerRightMouseUp;
            this.PreviewMouseDoubleClick += MarkerDoubleClicked;

            mPopup = new Popup {Placement = PlacementMode.Mouse};
            mLabel = new Label
            {
                Background = mDetections.Any(d => d.Material.MaterialType == MaterialType.Explosive) ? Brushes.Red : Brushes.Yellow,
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
        }

        private void MarkerDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (!Equals(mMainWindow.AddPointBtn.Background, Brushes.DarkBlue)) return;

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
