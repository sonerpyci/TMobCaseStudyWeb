using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace TMobCaseStudy.Base.Monitoring.Fake
{
    internal class TimerFake : ITimer
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;
        private readonly IDisposable _span;

        public string Name { get; }

        public TimeSpan Elapsed { get; private set; }

        public TimerFake(string name, ITracer tracer = null, ILogger logger = null)
        {
            Name = name;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _logger = logger;
            _span = tracer?.StartSpan(name);
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Elapsed = _stopwatch.Elapsed;
            _logger?.LogDebug("{0} ended in {1} ms.",
                Name, _stopwatch.ElapsedMilliseconds);
            _span?.Dispose();
        }
    }
}
