using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// This class represents abstract material
    /// </summary>
    public class Material : IEquatable<Material>
    {
        /// <summary>
        /// The name of the material
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The type of the material
        /// </summary>
        public MaterialType MaterialType { get; }

        /// <summary>
        /// The uniqe material ID
        /// </summary>
        public string Cas { get; }

        /// <summary>
        /// The Database ID
        /// </summary>
        internal string DatabaseId { get; }

        /// <summary>
        /// Material class constructor
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="materialType">The type of the material</param>
        /// <param name="cas">The uniqe material ID</param>
        /// <param name="databaseId">The database ID</param>
        public Material(string name, MaterialType materialType, string cas, string databaseId = "")
        {
            Name = name;
            MaterialType = materialType;
            Cas = cas;
            DatabaseId = databaseId;
        }

        public bool IsContainsMaterialType(List<string> materialTypeList)
        {
            return materialTypeList.Any(materialType => MaterialType.ToString().Equals(materialType));
        }

        public bool Equals(Material other)
        {
            if (other == null) return false;
            return Cas == other.Cas;
        }

        public override int GetHashCode()
        {
            return Cas.GetHashCode();
        }
    }
}
