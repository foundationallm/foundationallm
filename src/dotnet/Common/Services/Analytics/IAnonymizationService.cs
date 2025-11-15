namespace FoundationaLLM.Common.Services.Analytics
{
    /// <summary>
    /// Service for anonymizing user identifiers.
    /// </summary>
    public interface IAnonymizationService
    {
        /// <summary>
        /// Anonymizes a User Principal Name (UPN) using a secure hash.
        /// </summary>
        /// <param name="upn">The UPN to anonymize.</param>
        /// <returns>A hashed version of the UPN, or "anonymous" if the UPN is null or empty.</returns>
        string AnonymizeUPN(string? upn);

        /// <summary>
        /// Anonymizes a User ID using a secure hash.
        /// </summary>
        /// <param name="userId">The User ID to anonymize.</param>
        /// <returns>A hashed version of the User ID, or "anonymous" if the User ID is null or empty.</returns>
        string AnonymizeUserId(string? userId);
    }
}
