using System.Collections.Generic;
using System.Windows;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for OptionControl.xaml
    /// </summary>
    public partial class OptionControl
    {
        public OptionControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the MainWindow control
        /// </summary>
        /// <returns>The MainWindow or null on error</returns>
        private static MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        private void BtnAddCombination_OnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();

            new Window
            {
                Title = "Add new material combination", //DON'T CHANGE THE TITLE
                Content = new AddCombinationAlert(GetMainWindow().NumberOfMaterialsToShow),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void BtnShowCombinations_OnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();

            List<Material> materialsCombination = new List<Material>()
            {
                new Material("bla", MaterialType.Hazardous, "123-123"),
                new Material("ewrt", MaterialType.Safe, "123-123"),
                new Material("fdsgthrdfgdfgdfgdfgdfgth", MaterialType.Explosive, "123-123"),
            };

            var combinationsList = new List<Combination>
            {
                new Combination("Alertdfgdfgdfgdfgdfgdfg", materialsCombination),
                new Combination("Alert2", materialsCombination),
                new Combination("Alertdfgdfgdfgdfgdfgdfg", materialsCombination),
                new Combination("Alert2", materialsCombination),
                new Combination("Alertdfgdfgdfgdfgdfgdfg", materialsCombination),
                new Combination("Alert2", materialsCombination),
            };
            

            new Window
            {
                Title = "Showing all material combinations",
                Content = new ShowCombinationAlerts(combinationsList),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }
    }
}
