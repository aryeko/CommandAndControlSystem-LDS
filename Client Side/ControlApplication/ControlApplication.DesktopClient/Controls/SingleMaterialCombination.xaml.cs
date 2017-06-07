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
        private readonly List<string> _materialsToLoad;

        public SingleMaterialCombination(List<string> materialsToLoad)
        {
            InitializeComponent();
            _materialsToLoad = materialsToLoad;
        }

        /// <summary>
        /// Gets the MainWindow control
        /// </summary>
        /// <returns>The MainWindow or null on error</returns>
        private static MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        private void OnLoadedMaterialComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            MaterialComboBox.ItemsSource = _materialsToLoad;

            //Make the first item selected...
            MaterialComboBox.SelectedIndex = 0;
        }

        private void AddIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (!window.Title.Equals("Add new material combination")) continue;
                if (GetMainWindow().NumberOfMaterialsToShow >= 6) continue;

                GetMainWindow().NumberOfMaterialsToShow++;
                window.Content = new AddCombinationAlert(GetMainWindow().NumberOfMaterialsToShow);
            }
        }

        private void RemoveIcon_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (!window.Title.Equals("Add new material combination")) continue;
                if (GetMainWindow().NumberOfMaterialsToShow <= 1) continue;

                GetMainWindow().NumberOfMaterialsToShow--;
                window.Content = new AddCombinationAlert(GetMainWindow().NumberOfMaterialsToShow);
            }
        }
    }
}
