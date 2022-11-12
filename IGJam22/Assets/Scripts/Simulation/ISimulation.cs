namespace Simulation
{
    public interface ISimulation
    {
        /// <summary>
        /// Query a value from the simulation system
        /// </summary>
        /// <param name="influence">Which simulated influence to load</param>
        /// <param name="x">x coord (Island Coordinate space)</param>
        /// <param name="y">y coord (Island Coordinate space)</param>
        /// <param name="value">Output float value</param>
        /// <returns>Returns true if a value is currently available.</returns>
        public bool GetValue(Influence influence, int x, int y, out float value);
    }
}