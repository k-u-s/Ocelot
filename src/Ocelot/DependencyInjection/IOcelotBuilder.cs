using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Middleware.Multiplexer;
using System;
using System.Net.Http;

namespace Ocelot.DependencyInjection
{
    public interface IOcelotBuilder
    {
        IServiceCollection Services { get; }

        IConfiguration Configuration { get; }

        IOcelotBuilder AddDelegatingHandler<T>(bool global = false)
            where T : DelegatingHandler;

        IOcelotBuilder AddSingletonDefinedAggregator<T>()
            where T : class, IDefinedAggregator;

        IOcelotBuilder AddTransientDefinedAggregator<T>()
            where T : class, IDefinedAggregator;

        IOcelotBuilder AddPrimaryHttpClientHandler(string name, Func<DownstreamContext, HttpClientHandler> creator);

        IOcelotBuilder AddDefaultPrimaryHttpClientHandler(Func<DownstreamContext, HttpClientHandler> creator);
    }
}
