using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly Detection _detection;
        public SingleDetectionControl(Detection detection)
        {
            InitializeComponent();

            _detection = detection;
            dataDate.Content = detection.DateTimeOfDetection.ToString("d");
            dataTime.Content = detection.DateTimeOfDetection.ToString("T");
            dataSuspectedID.Content = detection.SuspectId;
            dataPlateID.Content = detection.SuspectPlateId;
            dataMaterial.Content = detection.Material.Name;
            dataGunID.Content = string.IsNullOrEmpty(detection.GunId) ? "No G-Scan" : detection.GunId;
            if (!string.IsNullOrEmpty(detection.RamanGraph))
            {
                dataLinkRaman.Text = "LINK";
                dataLinkRaman.TextDecorations = TextDecorations.Underline;
                dataLinkRaman.Foreground = Brushes.CornflowerBlue;
                dataLinkRaman.MouseUp += OpenRaman_OnMouseClick;
            }
        }

        private void OpenRaman_OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var ramanFilePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "raman.esp");
                File.WriteAllText(ramanFilePath, _detection.RamanGraph);

                var p = Process.Start(ramanFilePath);
                p.WaitForExit();
                if(p.ExitCode != 0)
                    MessageBox.Show(Window.GetWindow(this), "Please run NT as an administrator", "Raman Graph Error", MessageBoxButton.OK);
            }
            catch (Exception)
            {
                MessageBox.Show(Window.GetWindow(this), "ESP files are not supported in your computer, please install enspecter", "Raman Graph Error", MessageBoxButton.OK);
            }    
        }
    }
}
