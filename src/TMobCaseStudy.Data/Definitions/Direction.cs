namespace TMobCaseStudy.Data.Definitions;

public enum Direction : int
{
    North = 0,
    NorthEast = 45,
    East = 90,
    SouthEast = 135,
    South = 180,
    SouthWest = 225,
    West = 270,
    NorthWest = 315
}

public static class DirectionExtensions
{
    public static Direction FromInput(string directionInput)
    {
        return directionInput.ToUpperInvariant() switch
        {
            "N" => Direction.North,
            "W" => Direction.West,
            "S" => Direction.South,
            "E" => Direction.East,
            "NE" => Direction.NorthEast,
            "NW" => Direction.NorthWest,
            "SE" => Direction.SouthEast,
            "SW" => Direction.SouthWest,
            _ => throw new ArgumentOutOfRangeException("directionInput")
        };
    }
    
}