using Ocelot.Middleware;
using System;
using System.Net.Http;

namespace Ocelot.Requester
{
    public interface IPrimaryHttpClientHandlerFactory
    {
        HttpClientHandler Create(DownstreamContext context, string name = default(string));

        void Add(string name, Func<DownstreamContext, HttpClientHandler> manipulator);

        void AddDefault(Func<DownstreamContext, HttpClientHandler> manipulator);
    }
}
