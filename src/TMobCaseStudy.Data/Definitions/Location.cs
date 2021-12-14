using TMobCaseStudy.Data.Definitions.Exceptions;
using TMobCaseStudy.Data.Entities;

namespace TMobCaseStudy.Data.Definitions;

public class Location
{
    public int X { get; set; }
    public int Y { get; set; }
    public Direction Direction { get; set; }

    public Location(int x, int y, Direction direction)
    {
        X = x;
        Y = y;
        Direction = direction;
    }
}


public static class LocationExtensions
{
    public static void Validate(this Location location, Planet planet)
    {
        if (location.X > planet.HorizontalBoundary ||
            location.Y > planet.VerticalBoundary ||
            location.X < 0 ||
            location.Y < 0)
        {
            throw new OutOfPlanetException($"The Location is outside from planet with x : {location.X} and y: {location.Y}");
        }
    }

    public static Location CreateNewLocationUsingDirection(this Location current)
    {
        switch (current.Direction)
        {
            case Direction.North:
                return new Location(current.X, current.Y + 1, current.Direction);
            case Direction.East:
                return new Location(current.X + 1 , current.Y , current.Direction);
            case Direction.South:
                return new Location(current.X, current.Y - 1, current.Direction);
            case Direction.West:
                return new Location(current.X - 1, current.Y, current.Direction);
            default:
                return current;
        }
    }
    
}