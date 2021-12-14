using System.Collections.Concurrent;

namespace TMobCaseStudy.Base.PubSub
{
    public class PubSubFactory : IPubSubFactory
    {
        private readonly ConcurrentDictionary<string, IPubSub> _pubSubs =
            new ConcurrentDictionary<string, IPubSub>();

        public IPubSub Create(string topic)
        {
            return _pubSubs.GetOrAdd(topic, key => new PubSub());
        }
    }
}
