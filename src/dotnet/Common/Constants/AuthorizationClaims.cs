namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Claims that are used in the authorization process.
    /// </summary>
    public static class AuthorizationClaims
    {
        /// <summary>
        /// A replacement for appid. The application ID of the client using the token.
        /// The application can act as itself or on behalf of a user.
        /// The application ID typically represents an application object, but it can also represent a service principal object in Microsoft Entra ID.
        /// </summary>
        public const string ApplicationId = "azp";
    }
}
