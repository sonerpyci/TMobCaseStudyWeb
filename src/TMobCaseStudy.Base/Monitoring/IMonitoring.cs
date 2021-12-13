using Microsoft.Extensions.Logging;

namespace TMobCaseStudy.Base.Monitoring
{
    public interface IMonitoring
    {
        Task FlushAsync();

        ITimer NewTimer(string name);
        
        ITimer NewTimer(string name, ILogger logger);

        ITimer NewTimer(string name, ITracer tracer);

        ITimer NewTimer(string name, ITracer tracer, ILogger logger);

        ITimer NewTimer(string name, IDictionary<string, string> labels);

        ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer);

        ITimer NewTimer(string name, IDictionary<string, string> labels, ILogger logger);

        ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer, ILogger logger);

        ICounter NewCounter(string name);

        ICounter NewCounter(string name, IDictionary<string, string> labels);
        
        IHistogram NewHistogram(string name);

        IHistogram NewHistogram(string name, IDictionary<string, string> labels);

        void ReportError(LogLevel level, string message, Exception exception);

        void ReportError(LogLevel level, string message, Exception exception,
            string[] tags, IDictionary<string, object> properties);

        string Dump();
    }
}
