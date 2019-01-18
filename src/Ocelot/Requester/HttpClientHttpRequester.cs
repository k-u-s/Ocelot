using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Responses;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ocelot.Requester
{
    public class HttpClientHttpRequester : IHttpRequester
    {
        private readonly IHttpClientCache _cacheHandlers;
        private readonly IOcelotLogger _logger;
        private readonly IPrimaryHttpClientHandlerFactory _primaryFactory;
        private readonly IDelegatingHandlerHandlerFactory _factory;
        private readonly IExceptionToErrorMapper _mapper;

        public HttpClientHttpRequester(IOcelotLoggerFactory loggerFactory,
            IHttpClientCache cacheHandlers,
            IPrimaryHttpClientHandlerFactory primaryFactory,
            IDelegatingHandlerHandlerFactory factory,
            IExceptionToErrorMapper mapper)
        {
            _logger = loggerFactory.CreateLogger<HttpClientHttpRequester>();
            _cacheHandlers = cacheHandlers;
            _primaryFactory = primaryFactory;
            _factory = factory;
            _mapper = mapper;
        }

        public async Task<Response<HttpResponseMessage>> GetResponse(DownstreamContext context)
        {
            var builder = new HttpClientBuilder(_primaryFactory, _factory, _cacheHandlers, _logger);

            var httpClient = builder.Create(context);

            try
            {
                var response = await httpClient.SendAsync(context.DownstreamRequest.ToHttpRequestMessage());
                return new OkResponse<HttpResponseMessage>(response);
            }
            catch (Exception exception)
            {
                var error = _mapper.Map(exception);
                return new ErrorResponse<HttpResponseMessage>(error);
            }
            finally
            {
                builder.Save();
            }
        }
    }
}
