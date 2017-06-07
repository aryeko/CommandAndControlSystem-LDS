using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlApplication.Core.Networking;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for AddCombinationAlert.xaml
    /// </summary>
    public partial class AddCombinationAlert
    {
        public AddCombinationAlert(int numberOfRows)
        {
            InitializeComponent();

            var materialsToLoad = NetworkClientsFactory.GetNtServer().GetMaterial().Select(material => material.Name).ToList();
            materialsToLoad.Sort();

            for (var i=0; i< numberOfRows; i++)
            {
                var newRowIndex = MaterialsListScroll.RowDefinitions.Count;
                MaterialsListScroll.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                var newControl = new SingleMaterialCombination(materialsToLoad);

                Grid.SetRow(newControl, newRowIndex);
                MaterialsListScroll.Children.Add(newControl);
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            var chosenCombination = (from SingleMaterialCombination material in MaterialsListScroll.Children select material.MaterialComboBox.Text).ToList();

            if (!ValidateFields(chosenCombination))
                return;

            Window.GetWindow(this)?.Close();

            //TODO: Insert combination to DB
        }

        /// <summary>
        /// Validating the mandatory text boxes are field as requiered.
        /// Marking the missing fields
        /// </summary>
        /// <returns>True if all the mandatory fields are field, false otherwise</returns>
        private bool ValidateFields(List<string> chosenCombination)
        {
            MarkBoxes(TxtAlert, true);
            if (TxtAlert.Text.Equals(string.Empty))
            {
                MarkBoxes(TxtAlert);
                return false;
            }

            if (chosenCombination.Count != chosenCombination.Distinct().Count())
            {
                MessageBox.Show("Please don't choose duplicated materials", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Marks the given boxes in red or removing the red marks
        /// </summary>
        /// <param name="boxToMark">The boxes to mark</param>
        /// <param name="cleanMark">Set to true to remove markers, false to mark. [Default: false]</param>
        private static void MarkBoxes(Control boxToMark, bool cleanMark = false)
        {
            boxToMark.BorderBrush = cleanMark ? Brushes.DarkGray : Brushes.Red;
            boxToMark.Background = cleanMark ? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250, 212, 212));
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }
    }
}