namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Represents a service for interacting with an MCP (Model Context Protocol) server.
    /// </summary>
    public interface IMCPClientService
    {
        Task<object> Test();
    }
}
