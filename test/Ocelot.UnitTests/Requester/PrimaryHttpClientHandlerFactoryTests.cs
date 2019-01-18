using Microsoft.AspNetCore.Http;
using Moq;
using Ocelot.Middleware;
using Ocelot.Requester;
using Shouldly;
using System.Net.Http;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Requester
{
    public class PrimaryHttpClientHandlerFactoryTests
    {
        private readonly Mock<HttpContext> _httpContext;
        private readonly PrimaryHttpClientHandlerFactory _primaryFactory;
        private HttpClientHandler response;

        public PrimaryHttpClientHandlerFactoryTests()
        {
            _httpContext = new Mock<HttpContext>();
            _primaryFactory = new PrimaryHttpClientHandlerFactory();
        }

        [Fact]
        public void should_return_httpclienthandler()
        {
            this.Given(x => GivenHttpRequestWithoutHeaders(default(string)))
                           .Then(x => response.ShouldBeOfType<HttpClientHandler>())
                           .BDDfy();
        }

        [Fact]
        public void should_return_added_httpclienthandler()
        {
            var handler1 = new HttpClientHandler();
            var handler2 = new HttpClientHandler();
            _primaryFactory.Add("test1", c => handler1);
            _primaryFactory.Add("test2", c => handler2);

            this.Given(x => GivenHttpRequestWithoutHeaders("test1"))
                .Then(x => response.ShouldBe(handler1))
                .And(x => GivenHttpRequestWithoutHeaders("test2"))
                .Then(x => response.ShouldBe(handler2))
                .BDDfy();
        }

        [Fact]
        public void should_return_last_added_httpclienthandler()
        {
            var handler1 = new HttpClientHandler();
            var handler2 = new HttpClientHandler();
            _primaryFactory.Add("test1", c => handler1);
            _primaryFactory.Add("test1", c => handler2);

            this.Given(x => GivenHttpRequestWithoutHeaders("test1"))
                .Then(x => response.ShouldBe(handler2))
                .BDDfy();
        }

        [Fact]
        public void should_return_added_default_httpclienthandler()
        {
            var handler = new HttpClientHandler();
            _primaryFactory.AddDefault(c => handler);

            this.Given(x => GivenHttpRequestWithoutHeaders(default(string)))
                .Then(x => response.ShouldBe(handler))
                .BDDfy();
        }

        [Fact]
        public void should_return_default_httpclienthandler_when_name_incorrect()
        {
            var handler = new HttpClientHandler();
            var handler1 = new HttpClientHandler();
            var handler2 = new HttpClientHandler();
            _primaryFactory.AddDefault(c => handler);
            _primaryFactory.Add("test1", c => handler1);
            _primaryFactory.Add("test2", c => handler2);

            this.Given(x => GivenHttpRequestWithoutHeaders("test3"))
                .Then(x => response.ShouldBe(handler))
                .BDDfy();
        }

        private void GivenHttpRequestWithoutHeaders(string name)
        {
            response = _primaryFactory.Create(new DownstreamContext(_httpContext.Object), name);
        }
    }
}
