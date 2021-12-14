using System.Threading.Tasks;

namespace TMobCaseStudy.Base.PubSub
{
    public interface IPublisher
    {
        Task PublishAsync(object message);
    }
}
