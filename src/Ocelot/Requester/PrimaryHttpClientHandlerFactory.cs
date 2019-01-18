using Ocelot.Middleware;
using System.Net.Http;

namespace Ocelot.Requester
{
    public class PrimaryHttpClientHandlerFactory : IPrimaryHttpClientHandlerFactory
    {
        public HttpClientHandler Get(DownstreamContext downstreamReRoute)
        {
            return new HttpClientHandler();
        }
    }
}
