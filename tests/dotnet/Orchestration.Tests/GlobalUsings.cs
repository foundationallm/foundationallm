namespace FoundationaLLM.Orchestration.Tests;

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