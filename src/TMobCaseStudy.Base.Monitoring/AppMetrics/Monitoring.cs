using App.Metrics;
using Exceptionless;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Metrics.Counter;
using App.Metrics.Histogram;
using App.Metrics.Timer;

namespace TMobCaseStudy.Base.Monitoring.AppMetrics
{
    public class Monitoring : IMonitoring
    {
        public Monitoring(IMetricsRoot metrics, ExceptionlessClient exceptionlessClient)
        {
            Metrics = metrics;
            ExceptionlessClient = exceptionlessClient;
        }

        private IMetricsRoot Metrics { get; }

        private ExceptionlessClient ExceptionlessClient { get; }

        public Task FlushAsync()
        {
            return Task.WhenAll(Metrics.ReportRunner.RunAllAsync());
        }

        public ITimer NewTimer(string name)
        {
            return new Timer(Metrics, name, labels: null, tracer: null, logger: null);
        }

        public ITimer NewTimer(string name, ITracer tracer)
        {
            return new Timer(Metrics, name, labels: null, tracer: tracer, logger: null);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels)
        {
            return new Timer(Metrics, name, labels, tracer: null, logger: null);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer)
        {
            return new Timer(Metrics, name, labels, tracer: tracer, logger: null);
        }

        public ITimer NewTimer(string name, ILogger logger)
        {
            return new Timer(Metrics, name, labels: null, tracer: null, logger: logger);
        }

        public ITimer NewTimer(string name, ITracer tracer, ILogger logger)
        {
            return new Timer(Metrics, name, labels: null, tracer: tracer, logger: logger);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ILogger logger)
        {
            return new Timer(Metrics, name, labels, tracer: null, logger: logger);
        }

        public ITimer NewTimer(string name, IDictionary<string, string> labels, ITracer tracer, ILogger logger)
        {
            return new Timer(Metrics, name, labels, tracer, logger);
        }

        public ICounter NewCounter(string name)
        {
            return new Counter(Metrics, name, labels: null);
        }

        public ICounter NewCounter(string name, IDictionary<string, string> labels)
        {
            return new Counter(Metrics, name, labels);
        }

        public IHistogram NewHistogram(string name)
        {
            return new Histogram(Metrics, name, labels: null);
        }

        public IHistogram NewHistogram(string name, IDictionary<string, string> labels)
        {
            return new Histogram(Metrics, name, labels);
        }

        public void ReportError(LogLevel level, string message, Exception exception)
        {
            ReportError(level, message, exception, /*tags:*/null, /*properties:*/null);
        }

        public void ReportError(LogLevel level, string message,
            Exception exception, string[] tags, IDictionary<string, object> properties)
        {
            if (ExceptionlessClient == null || exception == null)
            {
                return;
            }

            var builder = exception.
                ToExceptionless(null, ExceptionlessClient).
                SetMessage(message);
            
            if (tags != null)
            {
                builder.AddTags(tags);
            }

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    builder.SetProperty(property.Key, property.Value);
                }
            }

            if (level == LogLevel.Critical)
            {
                builder.MarkAsCritical();
            }
            builder.Submit();
        }

        public string Dump()
        {
            var snapshot = Metrics.Snapshot.Get();
            var formatter = Metrics.OutputMetricsFormatters.FirstOrDefault(x =>
                x.MediaType.SubType.Contains("prometheus")) ??
                Metrics.DefaultOutputMetricsFormatter;

            using var stream = new MemoryStream();
            formatter.WriteAsync(stream, snapshot).ConfigureAwait(false);
            var report = Encoding.UTF8.GetString(stream.ToArray());
            return report;
        }
    }
}

