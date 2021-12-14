
using TMobCaseStudy.Public.IO;

namespace TMobCaseStudy.Public
{
    // The Class For Tests.
    public class MarsRoverClientFake : IMarsRoverClient
    {
        public Task<SimulateMarsRoversOutput> SimulateMarsRovers(SimulateMarsRoversInput searchTripsInput)
        {
            throw new NotImplementedException();
        }
    }
}
