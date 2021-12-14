using Microsoft.AspNetCore.Mvc;
using TMobCaseStudy.Core;
using TMobCaseStudy.Data.Definitions;
using TMobCaseStudy.Data.Definitions.Exceptions;
using TMobCaseStudy.Data.Entities.SpaceCraft;
using TMobCaseStudy.Public.IO;

namespace TMobCaseStudy.Web.Controllers;

[ApiController]
public class SpaceCraftController : ControllerBase
{
    private readonly ILogger<SpaceCraftController> _logger;
    private readonly LocationOperation _locationOperations;
    private readonly PlanetOperation _planetOperations;
    private readonly SpaceCraftOperation _spaceCraftOperations;

    public SpaceCraftController(ILogger<SpaceCraftController> logger)
    {
        _logger = logger;
        _locationOperations = new LocationOperation();
        _planetOperations = new PlanetOperation();
        _spaceCraftOperations = new SpaceCraftOperation();
    }

    [HttpPost]
    [Route("api/planet/simulate")]
    public SimulateMarsRoversOutput SimulateMarsRovers(SimulateMarsRoversInput simulateMarsRoversInput)
    {
        var roverFinishLocations = new List<string>();
        
        try
        {
            var planetSizeInputSection = simulateMarsRoversInput.InputList.FirstOrDefault();
            
            _planetOperations.ValidateInput(planetSizeInputSection);
            var (planetHorizontalBoundary, planetVerticalBoundary) = 
                _planetOperations.BuildPlanetBoundaries(planetSizeInputSection);
            
            var planet = _planetOperations.CreatePlanet(planetHorizontalBoundary, planetVerticalBoundary);
            
            // the line below, creates a list of (location, direction) pairs
            // every pair holds information of a single rover.
            var spaceCraftRoutingInputPairs = 
                _planetOperations.BuildRoversAndRoutesFromInput(simulateMarsRoversInput.InputList);
            
            ISpaceCraftBase spaceCraft = null;
            foreach (var (locationText, routingText) in spaceCraftRoutingInputPairs)
            {
                try
                {
                    var initialLocation = _locationOperations.CreateLocationFromLocationText(locationText);
                    // The line below, Validates if location outside of planet
                    initialLocation.Validate(planet);
                    
                    spaceCraft = _spaceCraftOperations.BuildSpaceCraft(initialLocation, planet, MotilityType.OnlyForward);
                    _spaceCraftOperations.MoveSpaceCraft(planet, spaceCraft, routingText);
                    roverFinishLocations.Add(spaceCraft.ToString());
                }
                catch (OutOfPlanetException e)
                {
                    if (spaceCraft != null)
                        roverFinishLocations.Add($"According to the given input, the rover gone out of planet. Last Known Location was : {spaceCraft}");
                }
            }

            return new SimulateMarsRoversOutput
            {
                Success = true,
                Message = "Process Finished.",
                RoverLocations = roverFinishLocations,
            };
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, simulateMarsRoversInput.ToString());
            return new SimulateMarsRoversOutput
            {
                Success = false,
                Message = e.Message
            };
        }
    }
}