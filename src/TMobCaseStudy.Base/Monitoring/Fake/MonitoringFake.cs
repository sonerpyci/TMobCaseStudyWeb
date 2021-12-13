using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Monitoring.Fake
{
    public class MonitoringFake : IMonitoring
    {
        public Task FlushAsync()
        {
            return Task.CompletedTask;
        }

        public ITimer NewTimer(string name)
        {
            return new TimerFake(name);
        }

        public ITimer NewTimer(string name, ITracer tracer)
        {
            return new TimerFake(name, tracer);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels)
        {
            return new TimerFake(name);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer)
        {
            return new TimerFake(name, tracer);
        }

        public ITimer NewTimer(string name, ILogger logger)
        {
            return new TimerFake(name, null, logger);
        }

        public ITimer NewTimer(string name, ITracer tracer, ILogger logger)
        {
            return new TimerFake(name, tracer, logger);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ILogger logger)
        {
            return new TimerFake(name, null, logger);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer, ILogger logger)
        {
            return new TimerFake(name, tracer, logger);
        }

        public ICounter NewCounter(string name)
        {
            return new CounterFake(name);
        }

        public ICounter NewCounter(string name, IDictionary<string, string> labels)
        {
            return new CounterFake(name);
        }
        
        public IHistogram NewHistogram(string name)
        {
            return new HistogramFake(name);
        }

        public IHistogram NewHistogram(string name, IDictionary<string, string> labels)
        {
            return new HistogramFake(name);
        }

        public void ReportError(LogLevel level, string message, Exception exception)
        {
        }

        public void ReportError(LogLevel level, string message, Exception exception,
            string[] tags, IDictionary<string, object> properties)
        {
        }

        public string Dump()
        {
            return "";
        }
    }
}
