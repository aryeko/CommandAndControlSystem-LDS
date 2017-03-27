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
    /// Interaction logic for SingleDetectionControl.xaml
    /// </summary>
    public partial class SingleDetectionControl : UserControl
    {
        public SingleDetectionControl(Detection detection)
        {
            InitializeComponent();

            dataDate.Content = detection.DateTimeOfDetection.ToString("d");
            dataTime.Content = detection.DateTimeOfDetection.ToString("T");
            dataSuspectedID.Content = detection.SuspectId;
            dataPlateID.Content = detection.SuspectPlateId;
            dataGunID.Content = detection.GunId;
            dataMaterial.Content = detection.Material.Name;
        }
    }
}
