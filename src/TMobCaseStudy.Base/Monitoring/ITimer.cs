using System;

namespace TMobCaseStudy.Base.Monitoring
{
    public interface ITimer : IDisposable
    {
        string Name { get; }

        TimeSpan Elapsed { get; }
    }
}
