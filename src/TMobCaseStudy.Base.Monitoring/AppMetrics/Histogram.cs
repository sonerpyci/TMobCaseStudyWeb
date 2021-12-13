using App.Metrics;
using App.Metrics.Histogram;
using System.Collections.Generic;

namespace TMobCaseStudy.Base.Monitoring.AppMetrics
{
    internal class Histogram : IHistogram
    {
        private readonly App.Metrics.Histogram.IHistogram _histogram;

        public string Name { get; }
        
        public Histogram(IMetrics metrics, string name, IDictionary<string, string> labels)
        {
            var options = new HistogramOptions()
            {
                Name = name,
                Tags = MetricsTagsBuilder.Build(labels)
            };
            _histogram = metrics.Provider.Histogram.Instance(options);
            Name = name;
        }

        public void Update(long value)
        {
            _histogram.Update(value);
        }

        public void Reset()
        {
            _histogram.Reset();
        }
    }
}
