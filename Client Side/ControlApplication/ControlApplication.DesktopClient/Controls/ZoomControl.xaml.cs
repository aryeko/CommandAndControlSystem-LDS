using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GMap.NET.WindowsPresentation;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ZoomControl.xaml
    /// </summary>
    public partial class ZoomControl : UserControl
    {
        private int mMaxZoom;
        private int mMinZoom;

        public GMapControl MapControl => (Application.Current.MainWindow as MainWindow)?.GMapControl;

        public ZoomControl()
        {
            InitializeComponent();
        }

        public void UpdateControl()
        {
            TxtZoom.Text = MapControl.Zoom.ToString(CultureInfo.CurrentCulture);
            mMaxZoom = MapControl.MaxZoom;
            mMinZoom = MapControl.MinZoom;
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            int currentZoom = Convert.ToInt32(TxtZoom.Text);

            if (currentZoom < mMaxZoom)
                SetZoom(currentZoom + 1);
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            int currentZoom = Convert.ToInt32(TxtZoom.Text);

            if (currentZoom > mMinZoom)
                SetZoom(currentZoom - 1);
        }

        private void SetZoom(int newZoom)
        {
            MapControl.Zoom = newZoom;
            TxtZoom.Text = newZoom.ToString();
        }
    }
}
