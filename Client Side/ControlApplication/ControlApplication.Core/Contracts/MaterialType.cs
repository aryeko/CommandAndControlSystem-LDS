using System;

namespace ControlApplication.Core.Contracts
{
    /// <summary>
    /// Enum that represents type of material
    /// </summary>
    [Flags]
    public enum MaterialType
    {
        None = 0,
        Supervision = 1,
        Forbidden = 2,
        Hazardous = 4,
        Flameable = 8,
        Explosive = 16,
        Alcohol = 32,
        Toxics = 64,
        Safe = 128
    }
}
