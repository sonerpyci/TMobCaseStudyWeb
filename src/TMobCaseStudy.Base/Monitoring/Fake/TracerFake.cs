using System;
using System.Collections.Generic;

namespace TMobCaseStudy.Base.Monitoring.Fake
{
    public class TracerFake : ITracer
    {
        public IDisposable StartSpan(string name, IDictionary<string, string> labels = null)
        {
            return new NoopDisposable();
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose() { }
        }
    }
}
