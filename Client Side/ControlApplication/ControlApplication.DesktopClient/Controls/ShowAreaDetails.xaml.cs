using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ShowAreaDetails.xaml
    /// </summary>
    public partial class ShowAreaDetails : UserControl
    {
        private Area mArea;
        private IEnumerable<Detection> mDetections; 

        public ShowAreaDetails(Area area, IEnumerable<Detection> detections = null)
        {
            InitializeComponent();

            mArea = area;
            mDetections = detections ?? new List<Detection>();
            AreaIcon.Source = new BitmapImage(new Uri($"/ControlApplication.DesktopClient;component/../Drawable/Icon_{mArea.AreaType}.png", UriKind.Relative));
            LabelHeader.Content = $"{mArea.AreaType} Area";
            LabelAreaRadius.Content = $"Area Effective Radius :  {mArea.Radius}";
            LabelAreaDetectionCount.Content = $"Area Detections Count :  {mDetections.Count()} ";
            this.StackPanel.Children.Add(new ShowMarkerDetections(mDetections));
        }
    }
}
