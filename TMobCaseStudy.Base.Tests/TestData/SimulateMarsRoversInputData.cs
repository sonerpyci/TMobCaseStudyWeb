using TMobCaseStudy.Public.IO;

namespace TMobCaseStudy.Base.Tests.TestData;

public class SimulateMarsRoversInputData
{
    public static readonly Lazy<SimulateMarsRoversInput> CorrectSimulateMarsRoversInput =
        new Lazy<SimulateMarsRoversInput>(() => 
            new SimulateMarsRoversInput{
                InputList = new List<string>
                {
                    "55",
                    "1 2 N",
                    "LMLMLMLMM",
                    "3 3 E",
                    "MMRMMRMRRM"
                }
            }, isThreadSafe: true);
    
    
    public static readonly Lazy<SimulateMarsRoversInput> WrongSimulateMarsRoversInput =
        new Lazy<SimulateMarsRoversInput>(() => 
            new SimulateMarsRoversInput{
                InputList = new List<string>
                {
                    "2020",
                    "3 3 E",
                    "MMRMMRMRRM",
                    "5 18 N",
                    "LMLMLMLMMMLMMLLLMMLMM",
                }
            }, isThreadSafe: true);
    
    public static readonly Lazy<SimulateMarsRoversInput> ZerosSimulateMarsRoversInput =
        new Lazy<SimulateMarsRoversInput>(() => 
            new SimulateMarsRoversInput{
                InputList = new List<string>
                {
                    "00",
                    "3 3 E",
                    "MMRMMRMRRM",
                    "5 6 S",
                    "LMLMLRMRLRMRLMMLMM",
                }
            }, isThreadSafe: true);
}