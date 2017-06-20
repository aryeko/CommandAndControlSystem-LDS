using System.Windows;
using ControlApplication.Core.Networking;

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

            var combinationsList = Networking.GetNtServer().GetMaterialsCombinationsAlerts();

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
