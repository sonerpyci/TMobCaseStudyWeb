using System;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Caching
{
    /// <summary>
    /// Cache interface.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// The name of the cache.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the object associated with the given key from the cache.
        /// </summary>
        /// <typeparam name="T">Type of the object to retrieve</typeparam>
        /// <param name="key">The key of the object</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Puts the given object/value in the cache.
        /// </summary>
        /// <typeparam name="T">Type of the object to put in the cache</typeparam>
        /// <param name="key">The key to put in the cache</param>
        /// <param name="value">The value to put in the cache</param>
        /// <param name="timeoutSeconds">Object removal timeout in seconds</param>
        Task PutAsync<T>(string key, T value, int timeoutSeconds) where T : class;

        /// <summary>
        /// Puts the given object/value in the cache.
        /// </summary>
        /// <typeparam name="T">Type of the object to put in the cache</typeparam>
        /// <param name="key">The key to put in the cache</param>
        /// <param name="value">The value to put in the cache</param>
        /// <param name="timeout">Object removal timeout</param>
        Task PutAsync<T>(string key, T value, TimeSpan timeout) where T : class;

        /// <summary>
        /// Removes the given key from the cache and returns true if the key existed before.
        /// </summary>
        /// <param name="key">The key to remove from the cache</param>
        Task<bool> RemoveAsync(string key);
    }
}
