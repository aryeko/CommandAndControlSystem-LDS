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
using ControlApplication.Core.Networking;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for SingleMaterialCombination.xaml
    /// </summary>
    public partial class SingleMaterialCombination : UserControl
    {
        public SingleMaterialCombination()
        {
            InitializeComponent();
        }

        private void OnLoadedMaterialComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var materialsToLoad = NetworkClientsFactory.GetNtServer().GetMaterial();
            var data = materialsToLoad.Select(material => material.Name).ToList();
            data.Sort();

            MaterialComboBox.ItemsSource = data;

            //Make the first item selected...
            MaterialComboBox.SelectedIndex = 0;
        }
    }
}
