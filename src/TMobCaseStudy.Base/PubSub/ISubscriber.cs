using System;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.PubSub
{
    public interface ISubscriber
    {
        Task SubscribeAsync<T>(Action<T> onMessageReceived);
    }
}
