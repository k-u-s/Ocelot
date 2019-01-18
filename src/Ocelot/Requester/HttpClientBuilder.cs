﻿using Ocelot.Configuration;
using Ocelot.Logging;
using Ocelot.Middleware;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Ocelot.Requester
{
    public class HttpClientBuilder : IHttpClientBuilder
    {
        private readonly IPrimaryHttpClientHandlerFactory _primaryFactory;
        private readonly IDelegatingHandlerHandlerFactory _factory;
        private readonly IHttpClientCache _cacheHandlers;
        private readonly IOcelotLogger _logger;
        private DownstreamReRoute _cacheKey;
        private HttpClient _httpClient;
        private IHttpClient _client;
        private readonly TimeSpan _defaultTimeout;

        public HttpClientBuilder(
            IPrimaryHttpClientHandlerFactory primaryFactory,
            IDelegatingHandlerHandlerFactory factory,
            IHttpClientCache cacheHandlers,
            IOcelotLogger logger)
        {
            _primaryFactory = primaryFactory;
            _factory = factory;
            _cacheHandlers = cacheHandlers;
            _logger = logger;

            // This is hardcoded at the moment but can easily be added to configuration
            // if required by a user request.
            _defaultTimeout = TimeSpan.FromSeconds(90);
        }

        public IHttpClient Create(DownstreamContext context)
        {
            _cacheKey = context.DownstreamReRoute;

            var httpClient = _cacheHandlers.Get(_cacheKey);

            if (httpClient != null)
            {
                _client = httpClient;
                return httpClient;
            }

            var handler = CreateHandler(context);

            if (context.DownstreamReRoute.DangerousAcceptAnyServerCertificateValidator)
            {
                handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, errors) => true;

                _logger
                    .LogWarning($"You have ignored all SSL warnings by using DangerousAcceptAnyServerCertificateValidator for this DownstreamReRoute, UpstreamPathTemplate: {context.DownstreamReRoute.UpstreamPathTemplate}, DownstreamPathTemplate: {context.DownstreamReRoute.DownstreamPathTemplate}");
            }

            var timeout = context.DownstreamReRoute.QosOptions.TimeoutValue == 0
                ? _defaultTimeout
                : TimeSpan.FromMilliseconds(context.DownstreamReRoute.QosOptions.TimeoutValue);

            _httpClient = new HttpClient(CreateHttpMessageHandler(handler, context.DownstreamReRoute))
            {
                Timeout = timeout
            };

            _client = new HttpClientWrapper(_httpClient);

            return _client;
        }

        private HttpClientHandler CreateHandler(DownstreamContext context)
        {
            // Dont' create the CookieContainer if UseCookies is not set or the HttpClient will complain
            // under .Net Full Framework
            var useCookies = context.DownstreamReRoute.HttpHandlerOptions.UseCookieContainer;

            if (useCookies)
            {
                return UseCookiesHandler(context);
            }
            else
            {
                return UseNonCookiesHandler(context);
            }
        }

        private HttpClientHandler UseNonCookiesHandler(DownstreamContext context)
        {
            var handler = _primaryFactory.Create(context, context.DownstreamReRoute.HttpHandlerOptions.PrimaryHandlerName);
            handler.AllowAutoRedirect = context.DownstreamReRoute.HttpHandlerOptions.AllowAutoRedirect;
            handler.UseCookies = context.DownstreamReRoute.HttpHandlerOptions.UseCookieContainer;
            handler.UseProxy = context.DownstreamReRoute.HttpHandlerOptions.UseProxy;
            return handler;
        }

        private HttpClientHandler UseCookiesHandler(DownstreamContext context)
        {
            var handler = _primaryFactory.Create(context, context.DownstreamReRoute.HttpHandlerOptions.PrimaryHandlerName);
            handler.AllowAutoRedirect = context.DownstreamReRoute.HttpHandlerOptions.AllowAutoRedirect;
            handler.UseCookies = context.DownstreamReRoute.HttpHandlerOptions.UseCookieContainer;
            handler.UseProxy = context.DownstreamReRoute.HttpHandlerOptions.UseProxy;
            handler.CookieContainer = new CookieContainer();
            return handler;
        }

        public void Save()
        {
            _cacheHandlers.Set(_cacheKey, _client, TimeSpan.FromHours(24));
        }

        private HttpMessageHandler CreateHttpMessageHandler(HttpMessageHandler httpMessageHandler, DownstreamReRoute request)
        {
            //todo handle error
            var handlers = _factory.Get(request).Data;

            handlers
                .Select(handler => handler)
                .Reverse()
                .ToList()
                .ForEach(handler =>
                {
                    var delegatingHandler = handler();
                    delegatingHandler.InnerHandler = httpMessageHandler;
                    httpMessageHandler = delegatingHandler;
                });
            return httpMessageHandler;
        }
    }
}
