namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides options for resource provider get operations.
    /// </summary>
    public class ResourceProviderGetOptions : ResourceProviderOperationOptionsBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether to load resource content (applicable only to resources that have content).
        /// </summary>
        public bool LoadContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include roles in the response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the value is set to <c>true</c>, for each resource, the response will include
        /// the roles assigned directly or indirectly to the resource.
        /// </para>
        /// </remarks>
        public bool IncludeRoles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include actions in the response.
        /// </summary>
        /// <remarks>
        /// If the value is set to <c>true</c>, for each resource, the response will include
        /// the authorizable actions assigned directly or indirectly to the resource.
        /// </remarks>
        public bool IncludeActions { get; set; }

        private static readonly HashSet<string> KnownParameters = new(StringComparer.OrdinalIgnoreCase)
        {
            "loadContent",
            "includeRoles",
            "includeActions"
        };

        /// <summary>
        /// Creates a new instance of the ResourceProviderGetOptions class by parsing option values from the specified
        /// query parameter dictionary.
        /// </summary>
        /// <remarks>Boolean option values are parsed using case-insensitive string representations of
        /// <see langword="true"/> or <see langword="false"/>. Unrecognized or missing parameters are ignored.</remarks>
        /// <param name="queryParams">A dictionary containing query parameter names and their corresponding values. Parameter names are expected
        /// to match option names such as "loadContent", "includeRoles", and "includeActions".</param>
        /// <param name="includeRolesDefault">The default value to use for the IncludeRoles option if it is not specified in the query parameters.</param>
        /// <param name="includeActionsDefault">The default value to use for the IncludeActions option if it is not specified in the query parameters.</param>
        /// <returns>A ResourceProviderGetOptions instance populated with values parsed from the query parameters. If a parameter
        /// is missing or cannot be parsed, the corresponding option retains its default value.</returns>
        public static ResourceProviderGetOptions FromQueryParams(
            Dictionary<string, string> queryParams,
            bool includeRolesDefault,
            bool includeActionsDefault)
        {
            var options = new ResourceProviderGetOptions
            {
                IncludeActions = includeActionsDefault,
                IncludeRoles = includeRolesDefault
            };

            if (queryParams is null)
                return options;

            if (queryParams.TryGetValue("loadContent", out var loadContentValue) &&
                bool.TryParse(loadContentValue, out var loadContent))
            {
                options.LoadContent = loadContent;
            }
            if (queryParams.TryGetValue("includeRoles", out var includeRolesValue) &&
                bool.TryParse(includeRolesValue, out var includeRoles))
            {
                options.IncludeRoles = includeRoles;
            }
            if (queryParams.TryGetValue("includeActions", out var includeActionsValue) &&
                bool.TryParse(includeActionsValue, out var includeActions))
            {
                options.IncludeActions = includeActions;
            }

            // Add unrecognized query parameters to the Parameters dictionary.
            foreach (var kvp in queryParams)
            {
                if (!KnownParameters.Contains(kvp.Key))
                {
                    options.Parameters[kvp.Key] = kvp.Value;
                }
            }

            return options;
        }

        /// <summary>
        /// Converts the current options instance to a query parameter dictionary.
        /// </summary>
        /// <returns>A dictionary containing query parameter names and their corresponding string values.</returns>
        public Dictionary<string, string> ToQueryParams()
        {
            var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (LoadContent)
                queryParams["loadContent"] = LoadContent.ToString().ToLowerInvariant();

            if (IncludeRoles)
                queryParams["includeRoles"] = IncludeRoles.ToString().ToLowerInvariant();

            if (IncludeActions)
                queryParams["includeActions"] = IncludeActions.ToString().ToLowerInvariant();

            return queryParams;
        }
    }
}
