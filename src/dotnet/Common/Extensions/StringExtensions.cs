namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends the <see cref="System.String"/> interface with helper methods.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a UPN (User Principal Name) to a string that is better suited to be used as an identifier.
        /// </summary>
        /// <param name="upn">The original UPN (User Principal Name).</param>
        /// <returns>A string containing the normalized UPN (User Principal Name).</returns>
        public static string NormalizeUserPrincipalName(
            this string upn) =>
            upn
                .Replace('.', '_')
                .Replace("#", ".")
                .Replace("@", "_")
                .ToLower();
    }
}
