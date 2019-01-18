using Ocelot.Middleware;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Ocelot.Requester
{
    public class PrimaryHttpClientHandlerFactory : IPrimaryHttpClientHandlerFactory
    {
        private const string DefaultNullPointerErrorMessage = "Method registred as default resturned null";
        private Func<DownstreamContext, HttpClientHandler> _defaultCreator;
        private readonly ConcurrentDictionary<string, Func<DownstreamContext, HttpClientHandler>> _primaryHandlers;

        public PrimaryHttpClientHandlerFactory()
        {
            _defaultCreator = c => new HttpClientHandler();
            _primaryHandlers = new ConcurrentDictionary<string, Func<DownstreamContext, HttpClientHandler>>();
        }

        public HttpClientHandler Create(DownstreamContext context, string name = default(string))
        {
            if (string.IsNullOrEmpty(name))
            {
                return EnsureNotNull(DefaultNullPointerErrorMessage, _defaultCreator(context));
            }

            return _primaryHandlers.ContainsKey(name)
                ? EnsureNotNull($"Method registered with name {name} returned null", _primaryHandlers[name](context))
                : EnsureNotNull(DefaultNullPointerErrorMessage, _defaultCreator(context));
        }

        public void Add(string name, Func<DownstreamContext, HttpClientHandler> creator)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _primaryHandlers[name] = creator ?? throw new ArgumentNullException(nameof(creator));
        }

        public void AddDefault(Func<DownstreamContext, HttpClientHandler> defaultCreator)
        {
            _defaultCreator = defaultCreator ?? throw new ArgumentNullException(nameof(defaultCreator));
        }

        private static HttpClientHandler EnsureNotNull(string msg, HttpClientHandler handler)
        {
            return handler ?? throw new NullReferenceException(msg);
        }
    }
}
