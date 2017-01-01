using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApplication.Core
{
    /// <summary>
    /// This class represents abstract material
    /// </summary>
    public class Material
    {
        /// <summary>
        /// The name of the material
        /// TODO: set the name acording to the DB
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the material
        /// </summary>
        public MaterialType MaterialType { get; }

        /// <summary>
        /// Material class constructor
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="materialType">The type of the material</param>
        public Material(string name, MaterialType materialType)
        {
            this.Name = name;
            this.MaterialType = materialType;
        }
    }
}
