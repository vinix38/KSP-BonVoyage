
namespace BonVoyage
{
    /// <summary>
    /// Informations about propellant for engines
    /// </summary>
    internal class Fuel
    {
        internal string Name; // Name of the propellant
        internal double FuelFlow; // Consumption per second
        internal double MaximumAmountAvailable; // Maximum amout of the propellant available for usage
        internal double CurrentAmountUsed; // Current amout of the propellant used by engines
    }
}
