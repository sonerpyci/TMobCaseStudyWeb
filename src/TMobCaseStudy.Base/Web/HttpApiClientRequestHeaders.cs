using System.Collections.Generic;

namespace TMobCaseStudy.Base.Web
{
    public class HttpApiClientRequestHeaders
    {
        public string CorrelationId { get; set; }

        public string AuthorizationKey { get; set; }

        public string CallerId { get; set; }

        public string CallerIp { get; set; }

        public IList<KeyValuePair<string, string>> CustomHeaders { get; set; }

        public HttpApiClientRequestHeaders() { }

        public HttpApiClientRequestHeaders(
            IList<KeyValuePair<string, string>> customHeaders)
        {
            CustomHeaders = customHeaders;
        }

        public HttpApiClientRequestHeaders(
            string correlationId, string authorizationKey,
            string callerId, string callerIp,
            IList<KeyValuePair<string, string>> customHeaders)
        {
            CorrelationId = correlationId;
            AuthorizationKey = authorizationKey;
            CallerId = callerId;
            CallerIp = callerIp;
            CustomHeaders = customHeaders;
        }
    }
}
