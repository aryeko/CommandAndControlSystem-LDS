using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for SingleCombinationList.xaml
    /// </summary>
    public partial class SingleCombinationList
    {
        public List<Material> CombinationMaterialsList { get; set; }

        public SingleCombinationList(string alertName, List<Material> combinationMaterialsList)
        {
            InitializeComponent();

            CombinationMaterialsList = combinationMaterialsList;
            LblAlertName.Text = alertName;
        }

        private void LblAlertName_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //var materialsCombination = NetworkClientsFactory.GetNtServer().GetCombination();

            if (CombinationDataXmal.Children.Count > 0)
                return;

            var newRowIndex = CombinationDataXmal.RowDefinitions.Count;
            CombinationDataXmal.RowDefinitions.Insert(newRowIndex, new RowDefinition());

            var newControl = new MaterialsList(CombinationMaterialsList);

            Grid.SetRow(newControl, newRowIndex);
            CombinationDataXmal.Children.Add(newControl);

        }

        private void LblAlertName_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CombinationDataXmal.Children.Clear();
        }
        
       
    }
}
