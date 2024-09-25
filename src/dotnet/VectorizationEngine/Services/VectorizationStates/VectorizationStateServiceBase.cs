using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Models;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Provides base services for the vectorization state services.
    /// </summary>
    public abstract class VectorizationStateServiceBase
    {
        /// <summary>
        /// Gets the persistence identifier of the vectorization state based on the vectorization request.
        /// </summary>
        /// <param name="vectorizationRequest">The <see cref="VectorizationRequest"/> for which the persistence identifier is being built.</param>
        /// <returns>The string version of the persistence identifier.</returns>
        protected string GetPersistenceIdentifier(VectorizationRequest vectorizationRequest) =>
            $"{vectorizationRequest.ContentIdentifier.CanonicalId}_{vectorizationRequest.PipelineName}_state_{HashContentIdentifier(vectorizationRequest.ContentIdentifier)}";

        /// <summary>
        /// Gets the persistence identifier of the vectorization state based on the vectorization state itself.
        /// </summary>
        /// <param name="vectorizationState">The <see cref="VectorizationState"/> for which the persistence identifier is being built.</param>
        /// <returns>The string version of the persistence identifier.</returns>
        protected string GetPersistenceIdentifier(VectorizationState vectorizationState) =>
            $"{vectorizationState.ContentIdentifier.CanonicalId}_{vectorizationState.PipelineName}_state_{HashContentIdentifier(vectorizationState.ContentIdentifier)}";

        /// <summary>
        /// Computes the MD5 hash of the content identifier.
        /// </summary>
        /// <param name="contentIdentifier">The <see cref="ContentIdentifier"/> holding the content identification information.</param>
        /// <returns>The string version of the hash.</returns>
        protected static string HashContentIdentifier(ContentIdentifier contentIdentifier)
        {
            var byteHash = MD5.HashData(
                Encoding.UTF8.GetBytes(
                    contentIdentifier.CanonicalId + "|" + contentIdentifier.UniqueId));

            return BitConverter.ToString(byteHash).Replace("-", string.Empty);
        }

        /// <summary>
        /// Computes the MD5 hash of a specified chunk of text.
        /// </summary>
        /// <param name="text">The chunk of text to be hashed.</param>
        /// <returns>The string version of the hash.</returns>
        protected static string HashText(string text) =>
            BitConverter
                .ToString(MD5.HashData(
                    Encoding.UTF8.GetBytes(text)))
                .Replace("-", string.Empty);
    }
}
