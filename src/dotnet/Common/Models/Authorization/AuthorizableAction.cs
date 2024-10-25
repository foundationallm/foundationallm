namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Defines the properties of an authorizable action managed by the FoundationaLLM.Authorization resource provider.
    /// </summary>
    /// <param name="Name">The name of the authorizable action.</param>
    /// <param name="Description">The description of the authorizable action.</param>
    /// <param name="Category">The category of the authorizable action.</param>
    public record AuthorizableAction(
        string Name,
        string Description,
        string Category)
    {
    }
}
