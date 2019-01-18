using Ocelot.Middleware;
using System.Net.Http;

namespace Ocelot.Requester
{
    public interface IPrimaryHttpClientHandlerFactory
    {
        HttpClientHandler Get(DownstreamContext downstreamReRoute);
    }
}
