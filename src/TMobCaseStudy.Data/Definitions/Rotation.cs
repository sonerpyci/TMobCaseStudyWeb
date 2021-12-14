using TMobCaseStudy.Data.Entities.SpaceCraft;

namespace TMobCaseStudy.Data.Definitions;

public enum Rotation : int
{
    L = -1,
    R = 1
}

public static class RotationExtensions
{
    public static Direction CreateDirectionFromRotation(this Rotation rotation, Direction currentDirection)
    {
        return (Direction) (((int)(currentDirection + (int)rotation * 90) + 360) % 360);
    }
    
}