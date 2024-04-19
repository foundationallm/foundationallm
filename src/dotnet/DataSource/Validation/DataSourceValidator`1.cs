using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Base validator for data sources.
    /// </summary>
    /// <typeparam name="T">The type of data source to validate.</typeparam>
    public class DataSourceValidator<T> : AbstractValidator<T> where T : DataSourceBase
    {
        /// <summary>
        /// Validates the value of a specified configuration reference.
        /// </summary>
        /// <param name="dataSource">The data source object being validated.</param>
        /// <param name="configurationKey">The name of the configuration reference being validated.</param>
        /// <returns>True if the value of the configuration reference is valid, False otherwise.</returns>
        protected bool ValidConfigurationReference(DataSourceBase dataSource, string configurationKey) =>
            dataSource.ConfigurationReferences!.ContainsKey(configurationKey)
            && !string.IsNullOrWhiteSpace(dataSource.ConfigurationReferences[configurationKey])
            && (string.Compare(
                $"FoundationaLLM:DataSources:{dataSource.Name}:{configurationKey}",
                dataSource.ConfigurationReferences![configurationKey]) == 0);
    }
}
