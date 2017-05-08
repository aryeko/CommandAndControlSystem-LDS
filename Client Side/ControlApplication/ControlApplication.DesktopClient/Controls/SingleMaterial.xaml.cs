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
    /// Interaction logic for SingleMaterial.xaml
    /// </summary>
    public partial class SingleMaterial : UserControl
    {
        public SingleMaterial(Material material)
        {
            InitializeComponent();

            dataName.Content = material.Name;
            dataCas.Content = material.Cas;
            dataType.Content = material.MaterialType.ToString();
        }
    }
}
