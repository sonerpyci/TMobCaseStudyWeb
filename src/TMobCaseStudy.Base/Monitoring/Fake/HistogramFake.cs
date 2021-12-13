namespace TMobCaseStudy.Base.Monitoring.Fake
{
    internal class HistogramFake : IHistogram
    {
        private const int HistogramSize = 100;
        private int[] _values = new int[HistogramSize];

        public string Name { get; }
        
        public HistogramFake(string name)
        {
            Name = name;
        }

        public void Update(long value)
        {
            lock (_values)
            {
                _values[value % HistogramSize]++;
            }
        }

        public void Reset()
        {
            _values = new int[HistogramSize];
        }
    }
}
