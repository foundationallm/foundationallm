namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of an action executed by a resource provider.
    /// </summary>
    /// <param name="IsSuccessResult">Indicates whether the action executed successfully or not.</param>
    public record ResourceProviderActionResult(
        bool IsSuccessResult)
    {
    }
}
