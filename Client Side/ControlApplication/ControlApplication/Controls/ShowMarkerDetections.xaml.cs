using System.Windows;
using System.Windows.Controls;
using ControlApplication.Core;


namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ShowMarkerDetections.xaml
    /// </summary>
    public partial class ShowMarkerDetections : UserControl
    {
        private const int HEIGHT_ROW_CELL = 40;
        private const int INITIAL_HEIGHT = 80;
        private const int MAX_HEIGHT = 120;
        public string numberOfDetections { get; set; }

        public ShowMarkerDetections(Detection[] detectionData)
        {
            InitializeComponent();

            int newHeight = detectionData.Length * HEIGHT_ROW_CELL;

            if (newHeight <= MAX_HEIGHT)
            {
                Height = INITIAL_HEIGHT + newHeight;
                NumOfDetections.Height = new GridLength(newHeight);
            }
            else
            {
                Height = MAX_HEIGHT + INITIAL_HEIGHT;
                NumOfDetections.Height = new GridLength(MAX_HEIGHT);
            }
        }
    }
}
