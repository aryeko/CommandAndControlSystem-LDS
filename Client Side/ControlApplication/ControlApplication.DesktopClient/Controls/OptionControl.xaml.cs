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

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for OptionControl.xaml
    /// </summary>
    public partial class OptionControl : UserControl
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
                Title = "Add new material combination",
                Content = new AddCombinationAlert(GetMainWindow().NumberOfMaterialsToShow),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize
            }.ShowDialog();
        }

        private void BtnShowCombinations_OnClick(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
            throw new NotImplementedException();
        }
    }
}
