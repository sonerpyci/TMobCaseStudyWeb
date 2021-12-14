using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.PubSub
{
    public class PubSub : IPubSub
    {
        private readonly ConcurrentBag<Action<object>> _onMessageReceivedCallbacks
            = new ConcurrentBag<Action<object>>();
        
        public Task PublishAsync(object message)
        {
            foreach (var onMessageReceived in _onMessageReceivedCallbacks)
            {
                onMessageReceived(message);
            }
            return Task.CompletedTask;
        }

        public Task SubscribeAsync<T>(Action<T> onMessageReceived)
        {
            _onMessageReceivedCallbacks.Add(message => { onMessageReceived((T) message); });
            return Task.CompletedTask;
        }
    }
}
