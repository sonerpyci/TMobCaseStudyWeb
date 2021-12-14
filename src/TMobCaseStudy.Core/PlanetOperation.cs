using System.Text;
using TMobCaseStudy.Data.Definitions.Exceptions;
using TMobCaseStudy.Data.Entities;

namespace TMobCaseStudy.Core;

public class PlanetOperation : IOperationBase
{
    public void ValidateInput(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
            throw new Exception("Planet Size(s) Cannot be identified. Please Check the provided input.");
    }
    
    public Planet CreatePlanet(int horizontalBoundary, int verticalBoundary, string name="mars")
    {
        return new Planet(name, horizontalBoundary, verticalBoundary);
    }
    
    public (int, int) BuildPlanetBoundaries(string text)
    {
        var parts = Split(text).ToList();
        var counter = 0;
        var first = "";
        var middle = text.Length / 2;
        while (first.Length < middle)
        {
            first += parts[counter] + " ";
            counter++;
        }
        var second = string.Join(" ", parts.Skip(counter));
        
        int.TryParse(first, out var xBoundary);
        int.TryParse(second, out var yBoundary);

        if (xBoundary == 0 || yBoundary == 0)
            throw new ZeroPlanetBoundaryException("Planet x or y boundary is set to 0. Impossible case.");
        
        return (xBoundary, yBoundary);
    }
    
    private IEnumerable<string> Split(string str)
    {
        var sb = new StringBuilder(str);
        var half1 = sb.Length/2;
        var half2 = sb.Length - half1;
        var halfOne = new char[half1];
        var halfTwo = new char[half2];
        sb.CopyTo(0, halfOne, 0, half1);
        sb.CopyTo(half1, halfTwo, 0, half2);
        sb.Clear();
        return new List<string>
        {
            new string(halfOne), new string(halfTwo)
        };
        
    }
    
    public void ValidatePlanetSizeInput(string planetSizeInputSection)
    {
        if (string.IsNullOrWhiteSpace(planetSizeInputSection))
            throw new Exception("Planet Size(s) Cannot be identified. Please Check the provided input.");
    }
    
    public IEnumerable<Tuple<string, string>> BuildRoversAndRoutesFromInput(List<string> inputList)
    {
        var locationInputList = inputList.Where(x => inputList.IndexOf(x) % 2 == 1);
        var routingInputList = inputList.Where(x => inputList.IndexOf(x) % 2 == 0)
            .Skip(1);
        
        return locationInputList.Zip(routingInputList, Tuple.Create);
    }

    
}