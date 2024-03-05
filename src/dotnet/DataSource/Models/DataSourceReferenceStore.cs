namespace FoundationaLLM.DataSource.Models
{
    /// <summary>
    /// Models the content of the data source reference store managed by the FoundationaLLM.DataSource resource provider.
    /// </summary>
    public class DataSourceReferenceStore
    {
        /// <summary>
        /// The list of all data sources registered in the system.
        /// </summary>
        public required List<DataSourceReference> DataSourceReferences { get; set; }

        /// <summary>
        /// Creates a string-based dictionary of <see cref="DataSourceReference"/> values from the current object.
        /// </summary>
        /// <returns>The string-based dictionary of <see cref="DataSourceReference"/> values from the current object.</returns>
        public Dictionary<string, DataSourceReference> ToDictionary() =>
            DataSourceReferences.ToDictionary<DataSourceReference, string>(ar => ar.Name);

        /// <summary>
        /// Creates a new instance of the <see cref="DataSourceReferenceStore"/> from a dictionary.
        /// </summary>
        /// <param name="dictionary">A string-based dictionary of <see cref="DataSourceReference"/> values.</param>
        /// <returns>The <see cref="DataSourceReferenceStore"/> object created from the dictionary.</returns>
        public static DataSourceReferenceStore FromDictionary(Dictionary<string, DataSourceReference> dictionary) =>
            new()
            {
                DataSourceReferences = [.. dictionary.Values]
            };
    }
}
