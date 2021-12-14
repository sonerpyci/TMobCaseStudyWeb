using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Collections.Concurrent;
using TMobCaseStudy.Base.Caching;
using TMobCaseStudy.Base.Serialization;

namespace TMobCaseStudy.Base.Web
{
    /// <summary>
    ///   https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
    /// </summary>
    public class HttpApiClient
    {
        public abstract class ExceptionBuilder
        {
            /// <summary>
            /// Override to build a custom exception.
            /// </summary>
            /// <param name="data">Response received from the server</param>
            /// <param name="statusCode">Response HTTP status code</param>
            /// <param name="serializer">The serializer that can be used to serialize data</param>
            public abstract Exception Build(string data,
                HttpStatusCode statusCode, Serializer serializer);
        }

        private const string ContentTypeHeaderName = "Content-Type";
        private const string ContentEncodingHeaderName = "Content-Encoding";
        private const string ContentEncodingGzip = "gzip";
        private const string AuthorizationHeaderName = "Authorization";
        private const string CorrelationIdHeaderName = "CorrelationId";
        private const string CallerIdHeaderName = "X-CallerId";
        private const string CallerIpHeaderName = "X-Forwarded-For";

        private class HttpClientKey
        {
            public HttpClientKey(TimeSpan timeout)
            {
                Timeout = timeout;
            }

            public TimeSpan Timeout { get; private set; }

            public override bool Equals(object obj)
            {
                return obj is HttpClientKey key &&
                       Timeout.Equals(key.Timeout);
            }

            public override int GetHashCode()
            {
                return -687834008 +
                    EqualityComparer<TimeSpan>.Default.GetHashCode(Timeout);
            }
        }

        private static readonly ConcurrentDictionary<HttpClientKey, HttpClient> _httpClients =
            new ConcurrentDictionary<HttpClientKey, HttpClient>();
        private static readonly Random _random = new Random();
        private readonly HttpClient _httpClient;
        private readonly MediaType _mediaType;
        private readonly Uri[] _baseUris;
        private readonly HttpApiClientRequestHeaders _defaultRequestHeaders;
        private readonly bool _compressRequest;
        private readonly bool _compressResponse;
        private readonly ExceptionBuilder _exceptionBuilder;
        private readonly Serializer _serializer;
        private readonly LRUCache _cache;
        private readonly IHttpApiClientInterceptor _interceptor;

        internal HttpApiClient(
            HttpMessageHandler httpMessageHandler,
            MediaType mediaType,
            string[] baseUris,
            HttpApiClientRequestHeaders defaultRequestHeaders,
            TimeSpan? timeout,
            bool compressRequest,
            bool compressResponse,
            ExceptionBuilder exceptionBuilder,
            HttpApiClientCachingOptions cachingOptions,
            IHttpApiClientInterceptor interceptor)
        {
            _httpClient = GetOrCreateHttpClient(httpMessageHandler,
                timeout != null ? timeout.Value : TimeSpan.FromSeconds(120));
            _mediaType = mediaType;
            _baseUris = baseUris.Select(x => new Uri(x)).ToArray();
            _defaultRequestHeaders = defaultRequestHeaders;
            _compressRequest = compressRequest;
            _compressResponse = compressResponse;
            _exceptionBuilder = exceptionBuilder;
            var cachingOptions1 = cachingOptions;
            _serializer = GetSerializer(mediaType);
            if (cachingOptions1 != null)
            {
                _cache = new LRUCache(cachingOptions1.MaxSizeInBytes);
            }
            _interceptor = interceptor;
        }

        private HttpClient GetOrCreateHttpClient(
            HttpMessageHandler httpMessageHandler, TimeSpan requestTimeout)
        {
            if (httpMessageHandler != null)
            {
                return new HttpClient(httpMessageHandler) { Timeout = requestTimeout };
            }

            var key = new HttpClientKey(requestTimeout);
            return _httpClients.GetOrAdd(key, x =>
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression =
                        DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                return new HttpClient(handler) { Timeout = requestTimeout };
            });
        }

        private static Serializer GetSerializer(MediaType mediaType)
        {
            if (mediaType == MediaType.ApplicationJson)
            {
                return new JsonSerializer(shouldIgnoreNullValues: true);
            }
            else if (mediaType == MediaType.ApplicationBson)
            {
                return SerializerFactory.Get(SerializationType.Bson);
            }
            else if (mediaType == MediaType.ApplicationXml || mediaType == MediaType.TextXml)
            {
                return SerializerFactory.Get(SerializationType.Xml);
            }
            else if (mediaType == MediaType.ApplicationProtoBuf)
            {
                return SerializerFactory.Get(SerializationType.ProtoBuf);
            }
            else
            {
                throw new NotSupportedException(
                    string.Format("MediaType {0} not supported.", mediaType));
            }
        }

        /// <summary>
        /// Returns in-memory LRU cache statistics
        /// </summary>
        public HttpApiClientCacheStatistics GetCacheStatistics()
        {
            if (_cache == null)
            {
                return null;
            }
            return new HttpApiClientCacheStatistics()
            {
                CacheSizeInMegabytes = _cache.SizeInBytes / (decimal)(1024 * 1024),
                HitRatio = _cache.HitRatio,
                TotalHits = _cache.TotalHits,
                TotalMisses = _cache.TotalMisses,
                TotalEvicts = _cache.TotalEvicts,
                StatisticsPerKey = _cache.Statistics
            };
        }

        /// <summary>
        /// HTTP GET
        /// </summary>
        public Task<T> GetAsync<T>(string relativeUri,
            HttpApiClientRequestHeaders requestHeaders = null,
            TimeSpan? expiration = null, IHttpApiClientInterceptor interceptor = null)
        {
            return GetInternalAsync<T>(relativeUri,
                /*parameters:*/null, requestHeaders, expiration, interceptor);
        }

        /// <summary>
        /// HTTP GET
        /// </summary>
        public Task<T> GetAsync<T>(string relativeUri,
            IDictionary<string, string> parameters,
            HttpApiClientRequestHeaders requestHeaders = null,
            TimeSpan? expiration = null, IHttpApiClientInterceptor interceptor = null)
        {
            return GetInternalAsync<T>(relativeUri,
                parameters, requestHeaders, expiration, interceptor);
        }

        private async Task<T> GetInternalAsync<T>(
            string relativeUri, IDictionary<string, string> parameters,
            HttpApiClientRequestHeaders requestHeaders,
            TimeSpan? expiration, IHttpApiClientInterceptor interceptor)
        {
            var httpRequestMessage = BuildRequestMessage(
                HttpMethod.Get, requestHeaders, relativeUri, parameters);
            
            var cacheKey = httpRequestMessage.RequestUri.PathAndQuery;
            if (TryGetFromCache<T>(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }

            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            return await
                ProcessResponseMessageAsync<T>(cacheKey, expiration, httpResponseMessage).
                ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        public async Task PostAsync<T>(string relativeUri,
            T request, HttpApiClientRequestHeaders requestHeaders = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var httpRequestMessage = BuildRequestMessage(
                HttpMethod.Post, requestHeaders, relativeUri, BuildContent(request));
            
            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var responseString = await httpResponseMessage.Content.ReadAsStringAsync().
                    ConfigureAwait(false);
                throw BuildException(httpResponseMessage.StatusCode, responseString);
            }
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        public async Task<T> PostAsync<T>(string relativeUri,
            HttpApiClientRequestHeaders requestHeaders = null,
            TimeSpan? expiration = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var cacheKey = relativeUri;
            if (TryGetFromCache<T>(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }

            var httpRequestMessage = BuildRequestMessage(
                HttpMethod.Post, requestHeaders, relativeUri);

            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            return await
                ProcessResponseMessageAsync<T>(cacheKey, expiration, httpResponseMessage).
                ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        public async Task PostAsync(string relativeUri,
            IDictionary<string, string> parameters,
            HttpApiClientRequestHeaders requestHeaders = null,
            TimeSpan? expiration = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var httpRequestMessage = BuildRequestMessage(
                HttpMethod.Post, requestHeaders, relativeUri, parameters);

            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var responseString = await httpResponseMessage.Content.ReadAsStringAsync().
                    ConfigureAwait(false);
                throw BuildException(httpResponseMessage.StatusCode, responseString);
            }
        }

        /// <summary>
        /// HTTP POST
        /// </summary>
        public async Task<TV> PostAsync<T, TV>(
            string relativeUri, T request,
            HttpApiClientRequestHeaders requestHeaders = null,
            string cacheKey = null, TimeSpan? expiration = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            if (TryGetFromCache<TV>(cacheKey, out var cachedValue))
            {
                return cachedValue;
            }

            var httpRequestMessage = BuildRequestMessage(HttpMethod.Post,
                requestHeaders, relativeUri, BuildContent(request));

            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            return await
                ProcessResponseMessageAsync<TV>(cacheKey, expiration, httpResponseMessage).
                ConfigureAwait(false);
        }

        private bool TryGetFromCache<T>(string cacheKey, out T value)
        {
            if (_cache != null && cacheKey != null &&
                _cache.TryGet(cacheKey, out var responseBytes))
            {
                value = _serializer.DeserializeFromBytes<T>(responseBytes);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// HTTP PUT
        /// </summary>
        public async Task PutAsync<T>(string relativeUri, T request,
            HttpApiClientRequestHeaders requestHeaders = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var httpRequestMessage = BuildRequestMessage(HttpMethod.Put,
                requestHeaders, relativeUri, BuildContent(request));
            
            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                var responseString = await httpResponseMessage.
                    Content.ReadAsStringAsync().ConfigureAwait(false);
                throw BuildException(httpResponseMessage.StatusCode, responseString);
            }
        }

        /// <summary>
        /// HTTP PUT
        /// </summary>
        public async Task<TV> PutAsync<T, TV>(string relativeUri,
            T request, HttpApiClientRequestHeaders requestHeaders = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var httpRequestMessage = BuildRequestMessage(HttpMethod.Put,
                requestHeaders, relativeUri, BuildContent(request));
            
            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            return await
                ProcessResponseMessageAsync<TV>(httpResponseMessage).
                ConfigureAwait(false);
        }

        /// <summary>
        /// HTTP DELETE
        /// </summary>
        public async Task<TV> DeleteAsync<TV>(string relativeUri,
            IDictionary<string, string> parameters,
            HttpApiClientRequestHeaders requestHeaders = null,
            IHttpApiClientInterceptor interceptor = null)
        {
            var httpRequestMessage = BuildRequestMessage(
                HttpMethod.Delete, requestHeaders, relativeUri, parameters);

            var httpResponseMessage = await
                SendAsync(httpRequestMessage, interceptor).
                ConfigureAwait(false);
            return await
                ProcessResponseMessageAsync<TV>(httpResponseMessage).
                ConfigureAwait(false);
        }

        private HttpContent BuildContent<T>(T obj)
        {
            if (_compressRequest)
            {
                var gzipSerializer = new GzipSerializer(_serializer);
                var ms = new MemoryStream();
                gzipSerializer.SerializeToStream(obj, ms, leaveOpen:true);
                ms.Position = 0;
                var content = new StreamContent(ms);
                content.Headers.Add(ContentTypeHeaderName, _mediaType.ToString());
                content.Headers.Add(ContentEncodingHeaderName, ContentEncodingGzip);
                return content;
            }
            else
            {
                return new StringContent(
                    _serializer.SerializeToString(obj),
                    Encoding.UTF8, _mediaType.ToString());
            }
        }

        private HttpRequestMessage BuildRequestMessage(
            HttpMethod httpMethod, HttpApiClientRequestHeaders requestHeaders,
            string relativeUri, HttpContent httpContent = null)
        {
            return BuildRequestMessage(httpMethod,
                requestHeaders, relativeUri, /*parameters:*/null, httpContent);
        }

        private HttpRequestMessage BuildRequestMessage(HttpMethod httpMethod,
            HttpApiClientRequestHeaders requestHeaders, string relativeUri,
            IDictionary<string, string> parameters, HttpContent httpContent = null)
        {
            var requestUri = ConstructUri(new Uri(GetBaseUri(), relativeUri), parameters);
            var httpRequestMessage = new HttpRequestMessage(httpMethod, requestUri);
            if (httpContent != null)
            {
                httpRequestMessage.Content = httpContent;
            }
            AddDefaultRequestHeaders(httpRequestMessage);
            AddRequestHeaders(httpRequestMessage, requestHeaders);
            return httpRequestMessage;
        }

        private Uri GetBaseUri()
        {
            if (_baseUris.Length == 1)
            {
                return _baseUris[0];
            }
            var index = _random.Next(0, _baseUris.Length);
            return _baseUris[index];
        }

        private void AddDefaultRequestHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType.ToString()));
            if (_compressResponse)
            {
                request.Headers.AcceptEncoding.Add(
                    new StringWithQualityHeaderValue("gzip"));
                request.Headers.AcceptEncoding.Add(
                    new StringWithQualityHeaderValue("deflate"));
            }
            AddRequestHeaders(request, _defaultRequestHeaders);
        }

        private static Uri ConstructUri(Uri uri, IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return uri;
            }
            var uriBuilder = new UriBuilder(uri)
            {
                Query = string.Join("&",
                    parameters.Select(p => string.Format("{0}={1}", p.Key, p.Value)))
            };
            return uriBuilder.Uri;
        }

        private static void AddRequestHeaders(HttpRequestMessage request,
            HttpApiClientRequestHeaders requestHeaders)
        {
            if (requestHeaders == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(requestHeaders.CorrelationId))
            {
                request.Headers.TryAddWithoutValidation(
                    CorrelationIdHeaderName, requestHeaders.CorrelationId);
            }
            if (!string.IsNullOrEmpty(requestHeaders.AuthorizationKey))
            {
                request.Headers.TryAddWithoutValidation(
                    AuthorizationHeaderName, requestHeaders.AuthorizationKey);
            }
            if (!string.IsNullOrEmpty(requestHeaders.CallerId))
            {
                request.Headers.TryAddWithoutValidation(
                    CallerIdHeaderName, requestHeaders.CallerId);
            }
            if (!string.IsNullOrEmpty(requestHeaders.CallerIp))
            {
                request.Headers.TryAddWithoutValidation(
                    CallerIpHeaderName, requestHeaders.CallerIp);
            }
            if (requestHeaders.CustomHeaders != null)
            {
                foreach (var header in requestHeaders.CustomHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage httpRequestMessage, IHttpApiClientInterceptor interceptor)
        {
            object globalCorrelationState = null;
            if (_interceptor != null)
            {
                globalCorrelationState = await _interceptor.
                    BeforeSendRequestAsync(httpRequestMessage);
            }
            object correlationState = null;
            if (interceptor != null)
            {
                correlationState = await interceptor.
                    BeforeSendRequestAsync(httpRequestMessage);
            }

            var httpResponseMessage = await _httpClient.
                SendAsync(httpRequestMessage).ConfigureAwait(false);

            if (interceptor != null)
            {
                await interceptor.
                    AfterReceiveResponseAsync(httpResponseMessage, correlationState);
            }
            if (_interceptor != null)
            {
                await _interceptor.
                    AfterReceiveResponseAsync(httpResponseMessage, globalCorrelationState);
            }
            return httpResponseMessage;
        }

        private Task<T> ProcessResponseMessageAsync<T>(
            HttpResponseMessage httpResponseMessage)
        {
            return ProcessResponseMessageAsync<T>(/*cacheKey:*/null,
                /*expiration:*/null, httpResponseMessage);
        }

        private async Task<T> ProcessResponseMessageAsync<T>(
            string cacheKey, TimeSpan? expiration,
            HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                if (cacheKey != null && _cache != null)
                {
                    var responseData = await httpResponseMessage.Content.ReadAsByteArrayAsync().
                        ConfigureAwait(false);
                    _cache.AddOrReplace(cacheKey, responseData, expiration);
                    return _serializer.DeserializeFromBytes<T>(responseData);
                }
                else
                {
                    var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().
                        ConfigureAwait(false);
                    return _serializer.DeserializeFromStream<T>(responseStream);
                }
            }
            else
            {
                if (cacheKey != null && _cache != null)
                {
                    _cache.Remove(cacheKey);
                }
                var responseString = await httpResponseMessage.Content.ReadAsStringAsync().
                    ConfigureAwait(false);
                throw BuildException(httpResponseMessage.StatusCode, responseString);
            }
        }
        
        private Exception BuildException(HttpStatusCode statusCode, string response)
        {
            return _exceptionBuilder.Build(response, statusCode, _serializer);
        }
    }
}
