namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// The types of security principals.
    /// </summary>
    public static class PrincipalTypes
    {
        /// <summary>
        /// The user security principal type.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// The group security principal type.
        /// </summary>
        public const string Group = "Group";

        /// <summary>
        /// The service principal security principal type.
        /// </summary>
        public const string ServicePrincipal = "ServicePrincipal";

        /// <summary>
        /// Determines if the specified principal type is valid.
        /// </summary>
        /// <param name="principalType">The name of the principal type to validate.</param>
        /// <returns>True if the specified principal type is valid.</returns>
        public static bool IsValidPrincipalType(string principalType) =>
            new string[] { User, Group, ServicePrincipal }.Contains(principalType);
    }
}
