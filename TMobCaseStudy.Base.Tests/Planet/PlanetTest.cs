using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using TMobCaseStudy.Base.Caching;
using TMobCaseStudy.Base.Serialization;
using TMobCaseStudy.Base.Tests.TestData;
using TMobCaseStudy.Core;
using TMobCaseStudy.Data.Definitions.Exceptions;
using TMobCaseStudy.Data.Entities;
using TMobCaseStudy.Data.Entities.SpaceCraft;
using TMobCaseStudy.Public.IO;
using Xunit;

namespace TMobCaseStudy.Base.Tests.Caching
{
    public class PlanetTest
    {
        private readonly SimulateMarsRoversInput _correctInput;
        private readonly SimulateMarsRoversInput _wrongInput;
        private readonly SimulateMarsRoversInput _zerosInput;
        private readonly PlanetOperation _planetOperations;
        private readonly SpaceCraftOperation _spaceCraftOperations;
        private readonly LocationOperation _locationOperations;

        public PlanetTest()
        {
            _correctInput = SimulateMarsRoversInputData.CorrectSimulateMarsRoversInput.Value;
            _wrongInput = SimulateMarsRoversInputData.WrongSimulateMarsRoversInput.Value;
            _zerosInput = SimulateMarsRoversInputData.ZerosSimulateMarsRoversInput.Value;
            _planetOperations = new PlanetOperation();
            _spaceCraftOperations = new SpaceCraftOperation();
            _locationOperations = new LocationOperation();
        }

        [Fact]
        public void ZeroBoundaryInput()
        {
            // Arrange
            
            var planetSizeInputSection = _zerosInput.InputList.FirstOrDefault();
            
            _planetOperations.ValidateInput(planetSizeInputSection);
            
            try
            {
                var (planetHorizontalBoundary, planetVerticalBoundary) = 
                    _planetOperations.BuildPlanetBoundaries(planetSizeInputSection);
                var planet = _planetOperations.CreatePlanet(planetHorizontalBoundary, planetVerticalBoundary);
            }
            catch (Exception e)
            {
                Assert.IsType<ZeroPlanetBoundaryException>(e);
            }
            
            
           
        }
    }
}
