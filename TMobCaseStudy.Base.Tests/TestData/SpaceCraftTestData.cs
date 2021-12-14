using System.Collections.Immutable;
using TMobCaseStudy.Data.Definitions;
using TMobCaseStudy.Data.Entities.SpaceCraft;

namespace TMobCaseStudy.Base.Tests.TestData;

public class SpaceCraftTestData
{
    public static readonly Lazy<ImmutableList<ISpaceCraftBase>> SpaceCrafts =
        new (() =>
        {
            var planet = PlanetTestData.Planet;
            return new List<ISpaceCraftBase>
            {
                new Rover("test-rover-1", planet.Value, new Location(1, 3, Direction.North)),
                new Rover("test-rover-2", planet.Value, new Location(2, 5, Direction.South)),
                new Rover("test-rover-3", planet.Value, new Location(9, 3, Direction.East)),
                new Rover("test-rover-4", planet.Value, new Location(5, 7, Direction.West)),
                new Rover("test-rover-5", planet.Value, new Location(3, 4, Direction.East)),
                new Rover("test-rover-6", planet.Value, new Location(1, 8, Direction.South)),
            }.ToImmutableList();
        }, isThreadSafe: true);

}