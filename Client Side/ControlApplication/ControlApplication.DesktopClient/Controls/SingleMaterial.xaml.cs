using System.Windows.Media;
using ControlApplication.Core.Contracts;

namespace ControlApplication.DesktopClient.Controls
{
    /// <summary>
    /// Interaction logic for SingleMaterial.xaml
    /// </summary>
    public partial class SingleMaterial
    {
        public SingleMaterial(Material material)
        {
            InitializeComponent();

            //foreach (Label l in Grid.Children)
            //    l.Foreground = GetBrush(material);

            dataName.Content = material.Name;
            dataCas.Content = material.Cas;
            dataType.Content = material.MaterialType.ToString();
            dataType.Foreground = GetBrush(material);
        }

        private static Brush GetBrush(Material material)
        {
            switch (material.MaterialType)
            {
                case MaterialType.Alcohol:
                    return Brushes.Black;
                case MaterialType.Explosive:
                    return Brushes.Red;
                case MaterialType.Hazardous:
                    return Brushes.Gold;
                case MaterialType.Safe:
                    return Brushes.DarkGreen;
                case MaterialType.Supervision:
                    return Brushes.DarkCyan;
                case MaterialType.Forbidden:
                    return Brushes.Indigo;
                case MaterialType.Flameable:
                    return Brushes.DarkRed;
                case MaterialType.Toxics:
                    return Brushes.LightSkyBlue;
            }
            return Brushes.Black;
        }

    }
}
