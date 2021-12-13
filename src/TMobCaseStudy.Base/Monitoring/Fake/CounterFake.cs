namespace TMobCaseStudy.Base.Monitoring.Fake
{
    internal class CounterFake : ICounter
    {
        private long _counter;

        public string Name { get; }
        
        public CounterFake(string name)
        {
            Name = name;
        }

        public void Increment()
        {
            _counter += 1;
        }

        public void IncrementBy(int amount)
        {
            _counter += amount;
        }

        public void Reset()
        {
            _counter = 0;
        }
    }
}
