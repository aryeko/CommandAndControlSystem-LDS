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
using ControlApplication.Core;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ShowMarkerDetections.xaml
    /// </summary>
    public partial class ShowMarkerDetections : UserControl
    {
        private const int HEIGHT_ROW_CELL = 40;
        private const int INITIAL_HEIGHT = 50;
        private const int MAX_HEIGHT = 120;
        public string NumberOfDetections { get; set; }

        /// <summary>
        /// Shows all the detections that refered to a marker
        /// </summary>
        /// <param name="detectionData"></param>
        public ShowMarkerDetections(IEnumerable<Detection> detectionData)
        {
            InitializeComponent();

            int newHeight = detectionData.Count() * HEIGHT_ROW_CELL;

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
        
            foreach (var detection in detectionData)
            {
                var newRowIndex = DetectionDataXmal.RowDefinitions.Count;
                DetectionDataXmal.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                SingleDetectionControl newControl = new SingleDetectionControl(detection);

                Grid.SetRow(newControl, newRowIndex);
                DetectionDataXmal.Children.Add(newControl);
            }
            
        }
    }
}
