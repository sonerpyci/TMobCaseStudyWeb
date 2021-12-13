namespace TMobCaseStudy.Base.Monitoring
{
    public interface IHistogram
    {
        void Update(long value);

        void Reset();
    }
}
