namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// This class represents abstract material
    /// </summary>
    public class Material
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
        /// Material class constructor
        /// </summary>
        /// <param name="name">The name of the material</param>
        /// <param name="materialType">The type of the material</param>
        /// <param name="cas">The uniqe material ID</param>
        public Material(string name, MaterialType materialType, string cas)
        {
            Name = name;
            MaterialType = materialType;
            Cas = cas;
        }
    }
}
