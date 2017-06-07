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

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for MaterialsList.xaml
    /// </summary>
    public partial class MaterialsList : UserControl
    {
        public MaterialsList(IEnumerable<Material> materials)
        {
            InitializeComponent();

            foreach (var material in materials)
            {
                var newRowIndex = MaterialDataXmal.RowDefinitions.Count;
                MaterialDataXmal.RowDefinitions.Insert(newRowIndex, new RowDefinition());

                var newControl = new SingleMaterial(material);

                Grid.SetRow(newControl, newRowIndex);
                MaterialDataXmal.Children.Add(newControl);
            }
        }
    }
}
