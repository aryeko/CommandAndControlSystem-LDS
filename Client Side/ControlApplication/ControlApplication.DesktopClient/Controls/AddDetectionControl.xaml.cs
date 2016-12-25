using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for AddDetectionControl.xaml
    /// </summary>
    public partial class AddDetectionControl : UserControl
    {
        private Point mClickPoint;

        public AddDetectionControl(Point clickPoint)
        {
            InitializeComponent();

            mClickPoint = clickPoint;
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
                //TODO: Add data to the DB (relative to MARKER_SIZE)
                GetMainWindow().AddMarker(mClickPoint);
                Window.GetWindow(this)?.Close();
            }
        }

        /// <summary>
        /// Validating the mandatory text boxes are field as requiered.
        /// Marking the missing fields
        /// </summary>
        /// <returns>True if all the mandatory fields are field, false otherwise</returns>
        private bool ValidateFields()
        {
            TextBox[] txtFieldsToVerify = {TxtTime, TxtDate, TxtMaterial, TxtSuspectId, TxtSuspectPlateNo};
            MarkBoxes(txtFieldsToVerify, true);

            List<TextBox> emptyBoxes = txtFieldsToVerify.Where(txtBox => txtBox.Text.Equals(string.Empty)).ToList();

            if (emptyBoxes.Any())
            {
                MarkBoxes(emptyBoxes);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Marks the given boxes in red or removing the red marks
        /// </summary>
        /// <param name="boxesToMark">The boxes to mark</param>
        /// <param name="cleanMark">Set to true to remove markers, false to mark. [Default: false]</param>
        private void MarkBoxes(IEnumerable<TextBox> boxesToMark, bool cleanMark = false)
        {
            foreach (var emptyBox in boxesToMark)
            {
                emptyBox.BorderBrush = cleanMark ? Brushes.DarkGray : Brushes.Red;
                emptyBox.Background = cleanMark? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250,212,212));
            }
        }

        /// <summary>
        /// Gets the MainWindow control
        /// </summary>
        /// <returns>The MainWindow or null on error</returns>
        private MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }
    }
}
