using App.Metrics;
using System.Collections.Generic;
using System.Linq;

namespace TMobCaseStudy.Base.Monitoring.AppMetrics
{
    internal class MetricsTagsBuilder
    {
        public static MetricTags Build(IDictionary<string, string> labels)
        {
            if (labels == null || labels.Count == 0) return new MetricTags();

            return new MetricTags(labels.Keys.ToArray(), labels.Values.ToArray());
        }
    }
}
