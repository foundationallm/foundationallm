using FoundationaLLM.Common.Models.Configuration.AppConfiguration;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// A catalog containing the configuration entries for the solution.
    /// </summary>
    public static class AppConfigurationCatalog
    {
        /// <summary>
        /// Returns the list of all the app configuration entries for this solution.
        /// </summary>
        /// <returns></returns>
        public static List<AppConfigurationEntry> GetAllEntries() => [];
        

        /// <summary>
        /// Returns the list of all the app configuration entries for this solution that are required for the given version.
        /// </summary>
        /// <param name="version">The current version of the caller.</param>
        /// <returns></returns>
        public static IEnumerable<AppConfigurationEntry> GetRequiredConfigurationsForVersion(string version)
        {
            // Extract the numeric part of the version, ignoring pre-release tags.
            var numericVersionPart = version.Split('-')[0];
            if (!Version.TryParse(numericVersionPart, out var currentVersion))
            {
                throw new ArgumentException($"Invalid version format for the provided version ({version}).", nameof(version));
            }

            // Compare based on the Major, Minor, and Build numbers only.
            return GetAllEntries().Where(entry =>
            {
                if (string.IsNullOrWhiteSpace(entry.MinimumVersion))
                {
                    return false;
                }

                var entryNumericVersionPart = entry.MinimumVersion.Split('-')[0];
                if (!Version.TryParse(entryNumericVersionPart, out var entryVersion))
                {
                    return false;
                }

                var entryVersionWithoutRevision = new Version(entryVersion.Major, entryVersion.Minor, entryVersion.Build);
                var currentVersionWithoutRevision = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);

                return entryVersionWithoutRevision <= currentVersionWithoutRevision;
            });
        }
    }
}
