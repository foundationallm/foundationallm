global using Xunit;
global using NSubstitute;
global using Newtonsoft.Json;
global using System.Net;
global using System.Text;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

public class FakeMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public FakeMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}