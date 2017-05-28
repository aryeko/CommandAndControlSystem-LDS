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
using ControlApplication.Core.Contracts;
using ControlApplication.Core.Networking;
using GMap.NET;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for AddAreaControl.xaml
    /// </summary>
    public partial class AddAreaControl : UserControl
    {
        private bool mLegalRadiusValue;
        private int mRadius;
        private PointLatLng mClickPoint;

        public AddAreaControl(Point clickPoint) 
            : this((Application.Current.MainWindow as MainWindow).GMapControl.FromLocalToLatLng((int)clickPoint.X, (int)clickPoint.Y))
        {
        }

        public AddAreaControl(PointLatLng clickPoint)
        {
            InitializeComponent();
            mLegalRadiusValue = true;
            mRadius = 100;
            mClickPoint = clickPoint;
        }

        private void ComboAreaType_OnLoaded(object sender, RoutedEventArgs e)
        {
            var data = Enum.GetNames(typeof(AreaType));

            ComboAreaType.ItemsSource = data;

            //Make the first item selected...
            ComboAreaType.SelectedIndex = 0;
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (mLegalRadiusValue)
            {
                Window.GetWindow(this)?.Close();
                var newArea = new Area(mClickPoint, (AreaType)Enum.Parse(typeof(AreaType), ComboAreaType.Text), mRadius);
                ServerConnectionManager.Instance.AddArea(newArea);

                (Application.Current.MainWindow as MainWindow).AddMarker(mClickPoint, new[] { newArea });

                if (CheckBoxSetWorkingArea.IsChecked.Value)
                {
                    (Application.Current.MainWindow as MainWindow).ActiveWorkingArea = newArea;
                }
            }
            
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this)?.Close();
        }

        private void TxtRadius_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = (sender as TextBox).Text;
            var outVal = 0;
            var isNumeric = int.TryParse(newText, out outVal);
            TxtRadius.BorderBrush = isNumeric ? Brushes.DarkGray : Brushes.Red;
            TxtRadius.Background = isNumeric ? Brushes.Transparent : new SolidColorBrush(Color.FromRgb(250, 212, 212));
            mLegalRadiusValue = isNumeric;
            mRadius = outVal;
        }
    }
}
