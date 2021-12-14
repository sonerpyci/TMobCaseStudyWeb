namespace TMobCaseStudy.Data.Definitions.Exceptions;

public class ZeroPlanetBoundaryException : Exception
{
    public override string Message { get; }

    public ZeroPlanetBoundaryException(string message) : base(message)
    {
        Message = message;
    }
    
}