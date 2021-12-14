using TMobCaseStudy.Data.Entities.SpaceCraft;

namespace TMobCaseStudy.Data.Definitions;

[Flags]
public enum MotilityType : byte
{
    
    /// <summary>
    /// The move ability means the spacecraft can move forward.
    /// </summary>
    OnlyForward = 1,
    
    /// <summary>
    /// The move ability means the spacecraft can move forward, backward, left and right.
    /// </summary>
    StandardFourWay = 2,
    
    /// <summary>
    /// The move ability means the spacecraft can move in any direction included diagonals.
    /// </summary>
    Flying = 4,
}


public static class MotilityTypeExtensions
{
    public static Type DecideSpaceCraftType(this MotilityType motilityType)
    {
        return motilityType switch
        {
            // MotilityType.Flying => typeof(Drone),
            // MotilityType.StandardFourWay => typeof(Spider),
            MotilityType.OnlyForward => typeof(Rover),
            _ => typeof(Rover)
        };
    }
}