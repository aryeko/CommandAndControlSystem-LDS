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
        
            for (int j = 0; j < detectionData.Length; j++)
            {
                DetectionDataXmal.RowDefinitions.Insert(j, new RowDefinition());

                SingleDetectionControl newControl = new SingleDetectionControl(detectionData[j]);

                Grid.SetRow(newControl, j);
                DetectionDataXmal.Children.Add(newControl);
            }
            
        }
    }
}
