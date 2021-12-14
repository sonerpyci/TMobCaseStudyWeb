using System.Text;
using TMobCaseStudy.Data.Definitions;
using TMobCaseStudy.Data.Entities;

namespace TMobCaseStudy.Core;

public class LocationOperation : IOperationBase
{
    public void ValidateInput(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            throw new Exception("Planet Size(s) Cannot be identified. Please Check the provided input.");
    }

    public Location CreateLocationFromLocationText(string locationText)
    {
        var splittedLocationInput = locationText.Split();
        if (!int.TryParse(splittedLocationInput[0], out var xCoordinate) || 
            !int.TryParse(splittedLocationInput[1], out var yCoordinate))
            throw new Exception("Initial Locations for a rover cannot be identified. Please Check the provided input.");
        
        var directionInput = splittedLocationInput.LastOrDefault();
        var direction = DirectionExtensions.FromInput(directionInput);
        
        return new Location(xCoordinate, yCoordinate, direction);
    }
    
}