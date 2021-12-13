using App.Metrics;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TMobCaseStudy.Base.Monitoring.AppMetrics
{
    internal class Timer : ITimer
    {
        private readonly IDisposable _span;
        private readonly TimerContext _timerContext;
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;

        public string Name { get; }

        public TimeSpan Elapsed { get; private set; }

        public Timer(IMetrics metrics, string name,
            IDictionary<string, string> labels,
            ITracer tracer, ILogger logger)
        {
            _span = tracer?.StartSpan(name, labels);
            var options = new TimerOptions()
            {
                Name = name,
                Tags = MetricsTagsBuilder.Build(labels)
            };
            var timer = metrics.Provider.Timer.Instance(options);
            _logger = logger;
            Name = name;
            _timerContext = timer.NewContext();
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Elapsed = _stopwatch.Elapsed;
            _logger?.LogInformation("{Name} ended in {TotalMilliseconds} ms.",
                Name, Math.Round(Elapsed.TotalMilliseconds, 2));
            _timerContext.Dispose();
            _span?.Dispose();
        }
    }
}
