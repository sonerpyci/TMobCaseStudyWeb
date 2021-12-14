namespace TMobCaseStudy.Base.PubSub
{
    public interface IPubSubFactory
    {
        IPubSub Create(string topic);
    }
}
