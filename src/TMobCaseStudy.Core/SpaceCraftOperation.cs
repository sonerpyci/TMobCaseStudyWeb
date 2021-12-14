using System.Text;
using TMobCaseStudy.Data.Definitions;
using TMobCaseStudy.Data.Entities;
using TMobCaseStudy.Data.Entities.SpaceCraft;

namespace TMobCaseStudy.Core;

public class SpaceCraftOperation : IOperationBase
{
    public void ValidateInput(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            throw new Exception("Planet Size(s) Cannot be identified. Please Check the provided input.");
    }

    public ISpaceCraftBase BuildSpaceCraft(Location initialLocation, Planet planet, MotilityType motilityType)
    {
        switch (motilityType)
        {
            case MotilityType.OnlyForward:
                return new Rover(name: "Rover_"+DateTime.Now.Ticks, planet,  initialLocation);
            //case MotilityType.StandardFourWay:
                //return new Spider();
            //case MotilityType.Flying:
                //return new Drone();
            default:
                return new Rover("Rover_"+DateTime.Now.Ticks, planet, initialLocation);
        }
    }
    public void MoveSpaceCraft(Planet planet, ISpaceCraftBase spaceCraft, string routingText)
    {
        foreach (var character in routingText.ToUpperInvariant())
        {
            if (Enum.TryParse(character.ToString(), out Rotation rotation))
            {
                spaceCraft.Rotate(rotation);
            }

            if (character == 'M')
            {
                spaceCraft.Move();
            }
        }
    }
    
    
}