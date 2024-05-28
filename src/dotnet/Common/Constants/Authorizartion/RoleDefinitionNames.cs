namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides the names of the role definitions managed by the FoundationaLLM.Authorization provider.
    /// </summary>
    public static class RoleDefinitionNames
    {
        /// <summary>
        /// Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC.
        /// </summary>
        public const string Role_Based_Access_Control_Administrator = "17ca4b59-3aee-497d-b43b-95dd7d916f99";

        /// <summary>
        /// View all resources without the possiblity of making any changes.
        /// </summary>
        public const string Reader = "00a53e72-f66e-4c03-8f81-7e885fd2eb35";

        /// <summary>
        /// Full access to manage all resources without the possiblity of assigning roles in FoundationaLLM RBAC.
        /// </summary>
        public const string Contributor = "a9f0020f-6e3a-49bf-8d1d-35fd53058edf";

        /// <summary>
        /// Manage access to FoundationaLLM resources.
        /// </summary>
        public const string User_Access_Administrator = "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618";

        /// <summary>
        /// Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.
        /// </summary>
        public const string Owner = "1301f8d4-3bea-4880-945f-315dbd2ddb46";

    }
}
