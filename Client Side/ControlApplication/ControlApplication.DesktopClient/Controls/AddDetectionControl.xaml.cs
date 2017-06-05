using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMapControl = GMap.NET.WindowsPresentation.GMapControl;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for AddDetectionControl.xaml
    /// </summary>
    public partial class AddDetectionControl : UserControl
    {
        private PointLatLng mClickPoint;
        private DateTime mCurrentDateTime;
        private IList<Detection> mDetectionData;

        public AddDetectionControl(Point clickPoint, IEnumerable<Detection> detectionData = null) 
            : this(GetMainWindow().GMapControl.FromLocalToLatLng((int)clickPoint.X, (int)clickPoint.Y), detectionData)
        {
        }

        public AddDetectionControl(PointLatLng clickPoint, IEnumerable<Detection> detectionData = null)
        {
            InitializeComponent();

            mClickPoint = clickPoint;
            mCurrentDateTime = DateTime.Now;
            TxtDate.Text = mCurrentDateTime.ToString("D");
            TxtTime.Text = mCurrentDateTime.ToString("T");
            TxtDate.IsReadOnly = true;
            TxtTime.IsReadOnly = true;
			mDetectionData = detectionData?.ToList() ?? new List<Detection>();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateFields())
            {
                Window.GetWindow(this)?.Close();
                var material = NetworkClientsFactory.GetNtServer().GetMaterial(name:MaterialComboBox.Text).First();
                Console.WriteLine($"Adding Detection using {GetMainWindow().ActiveWorkingArea.AreaType} area at {GetMainWindow().ActiveWorkingArea.RootLocation}");
                var area = GetMainWindow().ActiveWorkingArea;
                var detection = new Detection(mCurrentDateTime, material, mClickPoint, area, TxtSuspectId.Text, TxtSuspectPlateNo.Text);
                NetworkClientsFactory.GetNtServer().AddDetection(detection);
                GetMainWindow().AddMarker(mClickPoint, new []{detection});
            }
        }

        /// <summary>
        /// Validating the mandatory text boxes are field as requiered.
        /// Marking the missing fields
        /// </summary>
        /// <returns>True if all the mandatory fields are field, false otherwise</returns>
        private bool ValidateFields()
        {
            TextBox[] txtFieldsToVerify = {TxtTime, TxtDate, TxtSuspectId, TxtSuspectPlateNo};
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
        private static MainWindow GetMainWindow()
        {
            return Application.Current.MainWindow as MainWindow;
        }

        /// <summary>
        /// Combo Box event handler for item loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaterialComboBox_OnLoaded(object sender, RoutedEventArgs e)
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
