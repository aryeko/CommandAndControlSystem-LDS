using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
using ControlApplication.Core.Networking;
using CheckBox = System.Windows.Controls.CheckBox;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ChooseDetectionType.xaml
    /// </summary>
    public partial class ChooseDetectionType : UserControl
    {
        public ChooseDetectionType()
        {
            InitializeComponent();

            var j = 0;
            foreach (var material in Enum.GetValues(typeof(MaterialType)))
            {
                DetectionDataXmal.RowDefinitions.Insert(j, new RowDefinition());
                var cb = new CheckBox
                {
                    Name = material.ToString(),
                    Content = material.ToString(),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                };

                Grid.SetRow(cb, j);
                DetectionDataXmal.Children.Add(cb);

                j++;   
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var cbValues = new List<string>();

            foreach (CheckBox cb in DetectionDataXmal.Children)
            {
                if (cb.IsChecked != null && cb.IsChecked.Value)
                {
                    cbValues.Add(cb.Name);
                    Console.WriteLine(cb.Name);
                }
            }
            //ShowSelectedDetections(cbValues);
            Window.GetWindow(this)?.Close();
        }

        private void ShowSelectedDetections(List<string> cbValues)
        {
            //TODO: Show detections from DB

            var detectionsToShow = ServerConnectionManager.GetInstance().GetDetections();

        }
    }
}
