using TMobCaseStudy.Public.IO;


namespace TMobCaseStudy.Public
{
    /// <summary>
    /// Provides easy access to Mars Rover service.
    /// </summary>
    public interface IMarsRoverClient
    {
        /// <summary>
        /// Puts Information about the planet and rover(s)
        /// </summary>
        /// <param name="simulateMarsRoversInput">Specifies planet and rovers.</param>
        /// <returns>A List of rover locations</returns>
        Task<SimulateMarsRoversOutput> SimulateMarsRovers(SimulateMarsRoversInput simulateMarsRoversInput);
    }
}
