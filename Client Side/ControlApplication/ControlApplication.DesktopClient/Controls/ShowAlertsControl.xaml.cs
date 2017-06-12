using System.Collections.Generic;
using System.Windows.Controls;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ShowAlertsControl.xaml
    /// </summary>
    public partial class ShowAlertsControl : UserControl
    {
        public ShowAlertsControl(List<Alert> alertsList)
        {
            InitializeComponent();

            foreach (var alert in alertsList)
            {
                var newRowIndex = AlertDataXaml.RowDefinitions.Count;
                AlertDataXaml.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                var newControl = new SingleAlertControl(alert);

                Grid.SetRow(newControl, newRowIndex);
                AlertDataXaml.Children.Add(newControl);
            }
        }
    
    }
}
