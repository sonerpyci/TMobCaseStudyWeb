namespace TMobCaseStudy.Base.Monitoring
{
    public interface ICounter
    {
        string Name { get; }
        
        void Increment();

        void IncrementBy(int amount);
        
        void Reset();
    }
}
