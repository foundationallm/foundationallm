namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Extends the <see cref="System.Guid"/> interface with helper methods.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Converts a <see cref="System.Guid"/> to a base64 string.
        /// </summary>
        /// <param name="guid">The GUID value to be converted.</param>
        /// <returns></returns>
        public static string ToBase64String(this Guid guid) =>
            Convert.ToBase64String(guid.ToByteArray())
                .Replace("/", "-")
                .Replace("+", "-")
                .TrimEnd('=');
    }
}
