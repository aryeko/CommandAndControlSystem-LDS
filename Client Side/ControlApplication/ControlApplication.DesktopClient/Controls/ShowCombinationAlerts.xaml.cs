using System.Collections.Generic;
using System.Windows.Controls;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for ShowCombinationAlerts.xaml
    /// </summary>
    public partial class ShowCombinationAlerts
    {
        public ShowCombinationAlerts(List<Combination> combinationList)
        {
            InitializeComponent();


            foreach (var combination in combinationList)
            {
                var newRowIndex = AlertDataXaml.RowDefinitions.Count;
                AlertDataXaml.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                var newControl = new SingleCombinationList(newRowIndex+1, combination.AlertName, combination.CombinationMaterialsList);

                Grid.SetRow(newControl, newRowIndex);
                AlertDataXaml.Children.Add(newControl);
            }
        }
    }
}
