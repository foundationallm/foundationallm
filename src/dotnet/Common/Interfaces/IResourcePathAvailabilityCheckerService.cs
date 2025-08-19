using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines a mechanism to check the availability of a resource path in a certain context.
    /// </summary>
    /// <remarks>Implementations of this interface should provide logic to determine whether a given resource
    /// path is available for use, such as limiting the exposed resource provider capabilities in certain APIs.</remarks>
    public interface IResourcePathAvailabilityCheckerService
    {
        /// <summary>
        /// Determines whether the specified resource path is available for the given HTTP method.
        /// </summary>
        /// <param name="method">The HTTP method to check, such as <see cref="HttpMethod.Get"/> or <see cref="HttpMethod.Post"/>.</param>
        /// <param name="resourcePath">The resource path to evaluate for availability.</param>
        /// <returns><see langword="true"/> if the resource path is available for the specified HTTP method; otherwise, <see
        /// langword="false"/>.</returns>
        bool IsResourcePathAvailable(
            HttpMethod method,
            ResourcePath resourcePath);
    }
}
