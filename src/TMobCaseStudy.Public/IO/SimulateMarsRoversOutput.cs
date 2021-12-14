namespace TMobCaseStudy.Public.IO;

public class SimulateMarsRoversOutput
{
    public bool Success { get; set; }
    
    public string Message { get; set; }
    public List<string> RoverLocations { get; set; }
}