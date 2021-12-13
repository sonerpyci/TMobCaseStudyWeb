
using App.Metrics;
using App.Metrics.Counter;

namespace TMobCaseStudy.Base.Monitoring.AppMetrics
{
    internal class Counter : ICounter
    {
        private readonly App.Metrics.Counter.ICounter _counter;

        public string Name { get; }
        
        public Counter(IMetrics metrics, string name, IDictionary<string, string> labels)
        {
            var options = new CounterOptions()
            {
                Name = name,
                Tags = MetricsTagsBuilder.Build(labels)
            };
            _counter = metrics.Provider.Counter.Instance(options);
            Name = name;
        }

        public void Increment()
        {
            _counter.Increment();
        }

        public void IncrementBy(int amount)
        {
            _counter.Increment(amount);
        }
        
        public void Reset()
        {
            _counter.Reset();
        }
    }
}
