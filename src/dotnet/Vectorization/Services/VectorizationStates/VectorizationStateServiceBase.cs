using FoundationaLLM.Common.Models.Vectorization;
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
        /// Gets the persistence identifier of the vectorization state based on the content identifier.
        /// </summary>
        /// <param name="contentIdentifier">The <see cref="ContentIdentifier"/> holding the content identification information.</param>
        /// <returns>The string version of the persistence identifier.</returns>
        protected string GetPersistenceIdentifier(ContentIdentifier contentIdentifier) =>
            $"{contentIdentifier.CanonicalId}_state_{HashContentIdentifier(contentIdentifier)}";

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
