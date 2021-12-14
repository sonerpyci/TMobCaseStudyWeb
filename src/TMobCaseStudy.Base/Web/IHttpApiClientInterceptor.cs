using System.Net.Http;
using System.Threading.Tasks;

namespace TMobCaseStudy.Base.Web
{
    /// <summary>
    /// Intercepts <see cref="HttpApiClient"/> requests.
    /// </summary>
    public interface IHttpApiClientInterceptor
    {
        /// <summary>
        /// Called right before request is sent to the server.
        /// </summary>
        /// <param name="request">Request to be sent to the server</param>
        /// <returns>A correlation state to be passed back to OnResponseReceived</returns>
        Task<object> BeforeSendRequestAsync(HttpRequestMessage request);

        /// <summary>
        /// Called right after response is received from the server.
        /// </summary>
        /// <param name="response">Response received from the server</param>
        /// <param name="correlationState">Correlation state object returned by BeforeSendRequest</param>
        Task AfterReceiveResponseAsync(HttpResponseMessage response, object correlationState);
    }
}
