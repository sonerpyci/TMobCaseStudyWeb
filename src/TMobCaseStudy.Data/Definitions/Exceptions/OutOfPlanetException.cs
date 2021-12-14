namespace TMobCaseStudy.Data.Definitions.Exceptions;

public class OutOfPlanetException : Exception
{
    public override string Message { get; }

    public OutOfPlanetException(string message) : base(message)
    {
        Message = message;
    }
    
}