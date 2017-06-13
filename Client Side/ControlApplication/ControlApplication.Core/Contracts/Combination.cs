using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// A calss which represents a prohibited combination of materials
    /// </summary>
    public class Combination
    {
        /// <summary>
        /// The Alert name to pop
        /// </summary>
        public string AlertName { get; set; }

        /// <summary>
        /// List of material, while their mixture creates a dangerous substance
        /// </summary>
        public List<Material> CombinationMaterialsList { get; set; }

        public Combination(string alertName, List<Material> combinationMaterialsList)
        {
            AlertName = alertName;
            CombinationMaterialsList = combinationMaterialsList;
        }

        public bool ContainesCombination(List<Material> materials)
        {
            return !CombinationMaterialsList.Except(materials).Any();
        }
    }
}
