using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMobCaseStudy.Base.Serialization;
using TMobCaseStudy.Base.Web;
using TMobCaseStudy.Public.IO;

namespace TMobCaseStudy.Public
{
    public class MarsRoverClient : IMarsRoverClient
    {
        /// <summary>
        /// Builder for <see cref="IMarsRoverClient" />.
        /// </summary>
        public class Builder
        {
            private readonly string[] _baseUris;
            private readonly string _userName;
            private readonly string _userIp;
            private readonly string _correlationId;
            private bool _compressResponse;
            private MediaType _mediaType = MediaType.ApplicationJson;
            private TimeSpan? _timeout;

            /// <summary>
            /// Creates a builder instance and initializes the required fields using the arguments.
            /// </summary>
            public Builder(string[] baseUris, string userName, string userIp, string correlationId)
            {
                _baseUris = baseUris;
                _userName = userName;
                _userIp = userIp;
                _correlationId = correlationId;
            }

            /// <summary>
            /// Creates a builder instance and initializes the required fields using the arguments.
            /// </summary>
            public Builder(string baseUri, string userName, string userIp, string correlationId)
                : this(new[] { baseUri }, userName, userIp, correlationId) { }

            /// <summary>
            /// Sets http connection timeout. Default is 120 seconds.
            /// </summary>
            public Builder SetTimeout(TimeSpan timeout)
            {
                _timeout = timeout;
                return this;
            }

            /// <summary>
            /// Sets media type (application/json, application/bson etc).
            /// </summary>
            public Builder SetMediaType(MediaType mediaType)
            {
                _mediaType = mediaType;
                return this;
            }

            /// <summary>
            /// Request a compressed response from the service.
            /// </summary>
            public Builder CompressResponse()
            {
                _compressResponse = true;
                return this;
            }

            /// <summary>
            /// Builds the instance.
            /// </summary>
            public IMarsRoverClient Build()
            {
                return new MarsRoverClient(_baseUris, _mediaType,
                    _correlationId, _userName, _userIp, _timeout, _compressResponse);
            }
        }

        private const string SimulateMarsRoversUri = "api/planet/simulate/";
       

        private readonly HttpApiClient _httpApiClient;

        internal MarsRoverClient(string[] baseUris, MediaType mediaType,
            string correlationId, string userName, string userIp,
            TimeSpan? timeout, bool compressResponse)
        {
            var httpApiClientBuilder = new HttpApiClientBuilder(mediaType, baseUris).
                SetCorrelationId(correlationId).
                SetCallerId(userName).
                SetCallerIp(userIp);

            if (timeout != null)
            {
                httpApiClientBuilder.SetTimeout(timeout.Value);
            }
            if (compressResponse)
            {
                httpApiClientBuilder.CompressResponse();
            }
            _httpApiClient = httpApiClientBuilder.Build();
        }
        
        public async Task<SimulateMarsRoversOutput> SimulateMarsRovers(SimulateMarsRoversInput simulateMarsRoversInput)
        {
            return await _httpApiClient.
                PostAsync<SimulateMarsRoversInput, SimulateMarsRoversOutput>(SimulateMarsRoversUri, simulateMarsRoversInput).
                ConfigureAwait(false);
        }
    }
}
