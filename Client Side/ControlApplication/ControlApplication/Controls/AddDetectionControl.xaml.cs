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
    /// Interaction logic for AddDetectionControl.xaml
    /// </summary>
    public partial class AddDetectionControl : UserControl
    {
        public AddDetectionControl()
        {
            InitializeComponent();

            TxtDate.Text = DateTime.Today.ToString("D");
            TxtTime.Text = DateTime.Now.ToString("T");
            //TODO: Set GunId & Location automaticlly too
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            
            if (ValidateFields())
            {
                //TODO: Add marker
            }
        }

        private bool ValidateFields()
        {
            TextBox[] txtFieldsToVerify = {TxtTime, TxtDate, TxtMaterial, TxtSuspectId, TxtSuspectPlateNo};
            MarkBoxes(txtFieldsToVerify, true);

            IEnumerable<TextBox> emptyBoxes = txtFieldsToVerify.Where(txtBox => txtBox.Text.Equals(string.Empty));

            if (emptyBoxes.Any())
            {
                MarkBoxes(emptyBoxes);
                return false;
            }
            return true;
        }

        private void MarkBoxes(IEnumerable<TextBox> emptyBoxes, bool cleanMark = false)
        {
            foreach (var emptyBox in emptyBoxes)
            {
                emptyBox.BorderBrush = cleanMark ? Brushes.DarkGray : Brushes.Red;
                emptyBox.Background = cleanMark? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250,212,212));
            }
        }
    }
}
