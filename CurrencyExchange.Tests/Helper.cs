using Moq;
using Moq.Protected;

namespace CurrencyExchange.Tests
{
    public static class MoqHttpClientExtensions
    {
        public static void SetupSendAsync(this Mock<HttpMessageHandler> handlerMock, string expectedUrl, HttpResponseMessage response)
        {
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri != null &&
                        req.RequestUri.ToString() == expectedUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }
    }

}
