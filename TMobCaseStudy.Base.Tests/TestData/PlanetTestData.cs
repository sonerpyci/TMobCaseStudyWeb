namespace TMobCaseStudy.Base.Tests.TestData;

public static class PlanetTestData
{
    public static readonly Lazy<Data.Entities.Planet> Planet =
        new Lazy<Data.Entities.Planet>(() => new Data.Entities.Planet("testPlanet", 10, 10), isThreadSafe: true);
}