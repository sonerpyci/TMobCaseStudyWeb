using System;
using System.Collections.Generic;

namespace TMobCaseStudy.Base.Monitoring
{
    public interface ITracer
    {
        IDisposable StartSpan(string name, IDictionary<string, string> labels = null);
    }
}
