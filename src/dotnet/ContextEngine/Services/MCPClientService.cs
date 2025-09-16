using FoundationaLLM.Context.Interfaces;
using ModelContextProtocol.Client;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides functionality for interacting with an MCP (Model Context Protocol) server.
    /// </summary>
    public class MCPClientService : IMCPClientService
    {
        public async Task<object> Test()
        {
            var transport = new SseClientTransport(new()
            {
                Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
            });

            var client = await McpClientFactory.CreateAsync(transport);

            var tools = await client.ListToolsAsync();

            var args = new Dictionary<string, object?>
            {
                ["query"] = ".NET 8 parallel execution of async methods"
            };

            var result = await client.CallToolAsync("microsoft_docs_search", args);

            return result;
        }
    }
}
