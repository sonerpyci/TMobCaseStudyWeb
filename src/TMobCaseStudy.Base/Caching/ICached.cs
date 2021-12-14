using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    public interface ICached<T> where T : class
    {
        string CacheName { get; }

        string KeyName { get; }

        Task<T> GetValueAsync();

        Task RefreshAsync();

        Task ClearAsync();
    }
}
