using TMobCaseStudy.Base.Collections;

namespace TMobCaseStudy.Base.Web
{
    public class HttpApiClientBuilder
    {
        private HttpMessageHandler _httpMessageHandler;
        private readonly MediaType _mediaType;
        private readonly string[] _baseUris;
        private string _correlationId;
        private string _authorizationKey;
        private string _callerId;
        private string _callerIp;
        private readonly List<KeyValuePair<string, string>> _customHeaders =
            new List<KeyValuePair<string, string>>();
        private TimeSpan? _timeout;
        private bool _compressRequest;
        private bool _compressResponse;
        private HttpApiClient.ExceptionBuilder _exceptionBuilder;
        private HttpApiClientCachingOptions _cachingOptions;
        private IHttpApiClientInterceptor _interceptor;
        
        public HttpApiClientBuilder(MediaType mediaType, string[] baseUris)
        {
            _mediaType = mediaType;
            _baseUris = baseUris ?? throw new ArgumentNullException(nameof(baseUris));
        }

        public HttpApiClientBuilder(MediaType mediaType, string baseUri)
            : this(mediaType, new[] { baseUri })
        {
            if (string.IsNullOrEmpty(baseUri))
            {
                throw new ArgumentNullException(nameof(baseUri));
            }
        }

        public HttpApiClientBuilder AddCustomRequestHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }
            _customHeaders.Add(new KeyValuePair<string, string>(name, value));
            return this;
        }

        public HttpApiClientBuilder SetCorrelationId(string correlationId)
        {
            if (string.IsNullOrEmpty(correlationId))
            {
                throw new ArgumentNullException(nameof(correlationId));
            }
            _correlationId = correlationId;
            return this;
        }

        public HttpApiClientBuilder SetAuthorizationKey(string authorizationKey)
        {
            if (string.IsNullOrEmpty(authorizationKey))
            {
                throw new ArgumentNullException(nameof(authorizationKey));
            }
            _authorizationKey = authorizationKey;
            return this;
        }

        public HttpApiClientBuilder SetCallerId(string callerId)
        {
            if (string.IsNullOrEmpty(callerId))
            {
                throw new ArgumentNullException(nameof(callerId));
            }
            _callerId = callerId;
            return this;
        }

        public HttpApiClientBuilder SetCallerIp(string callerIp)
        {
            if (string.IsNullOrEmpty(callerIp))
            {
                throw new ArgumentNullException(nameof(callerIp));
            }
            _callerIp = callerIp;
            return this;
        }

        public HttpApiClientBuilder SetTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public HttpApiClientBuilder CompressRequest()
        {
            _compressRequest = true;
            return this;
        }

        public HttpApiClientBuilder CompressResponse()
        {
            _compressResponse = true;
            return this;
        }

        public HttpApiClientBuilder SetExceptionBuilder(
            HttpApiClient.ExceptionBuilder exceptionBuilder)
        {
            _exceptionBuilder = exceptionBuilder;
            return this;
        }

        public HttpApiClientBuilder SetCachingOptions(
            HttpApiClientCachingOptions cachingOptions)
        {
            _cachingOptions = cachingOptions;
            return this;
        }

        public HttpApiClientBuilder SetInterceptor(
            IHttpApiClientInterceptor interceptor)
        {
            _interceptor = interceptor;
            return this;
        }

        public HttpApiClientBuilder SetHttpMessageHandler(
            HttpMessageHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
            return this;
        }
        
        public HttpApiClient Build()
        {
            var defaultRequestHeaders = BuildDefaultRequestHeaders();
            return new HttpApiClient(
                _httpMessageHandler, _mediaType, _baseUris,
                defaultRequestHeaders, _timeout,
                _compressRequest, _compressResponse,
                _exceptionBuilder, _cachingOptions, _interceptor);
        }

        private HttpApiClientRequestHeaders BuildDefaultRequestHeaders()
        {
            if (_correlationId == null && _authorizationKey == null &&
                _callerId == null && _callerIp == null && _customHeaders.IsNullOrEmpty())
            {
                return null;
            }
            return new HttpApiClientRequestHeaders(_correlationId,
                _authorizationKey, _callerId, _callerIp, _customHeaders);
        }
    }
}