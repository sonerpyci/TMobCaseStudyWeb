using TMobCaseStudy.Data.Definitions;

namespace TMobCaseStudy.Data.Entities.SpaceCraft;

public class SpaceCraftBase
{
    public string Name { get; set; }
    
    public MotilityType MotilityType { get; set; }
    
    public Location Location { get; set; }
    
    public Planet Planet { get; set; }
    
    public SpaceCraftBase(string name, Planet planet, Location location)
    {
        Name = name;
        Planet = planet;
        Location = location;
    }

    public override string ToString()
    {
        return $"{Location.X} {Location.Y} {Location.Direction.ToString()}";
    }
}