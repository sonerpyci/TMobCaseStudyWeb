using TMobCaseStudy.Data.Definitions;

namespace TMobCaseStudy.Data.Entities.SpaceCraft;

public class Rover : SpaceCraftBase, ISpaceCraftBase
{
    public Rover(string name, Planet planet, Location location) : base(name, planet, location)
    {
        MotilityType = MotilityType.OnlyForward;
    }

    public void Rotate(Rotation rotation)
    {
        Location.Direction = rotation.CreateDirectionFromRotation(Location.Direction);
    }

    public void Move()
    {
        var newLocation = Location.CreateNewLocationUsingDirection();
        newLocation.Validate(Planet);
        Location = newLocation;
    }
}