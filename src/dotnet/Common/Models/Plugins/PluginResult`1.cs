namespace FoundationaLLM.Common.Models.Plugins
{
    /// <summary>
    /// The result of an operation executed by a plugin.
    /// </summary>
    /// <param name="Value">The result of the operation.</param>
    /// <param name="Success">Indicates whether the operation executed successfully or not.</param>
    /// <param name="StopProcessing">Indicates whether further processing should stop or not.</param>
    /// <param name="ErrorMessage">When IsSuccess is false, contains an error message with details.</param>
    /// <remarks>
    /// If <paramref name="StopProcessing"/> is <see langword="true"/>, the caller of the plugin should not attempt again to ask
    /// the plugin to process the same work item. The plugin will set this property to true if it has
    /// identified a permanent error that will not be resolved by retrying the operation.
    /// </remarks>
    public record PluginResult<T>(
        T Value,
        bool Success,
        bool StopProcessing,
        string? ErrorMessage = null)
        where T : class
    {
    }
}
