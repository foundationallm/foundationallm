using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides configuration for filtering <see cref="APIEndpointConfiguration"/> resources in the FoundationaLLM.Configuration resource provider.
    /// </summary>
    public class APIEndpointConfigurationFilter : ResourceFilter
    {
        /// <summary>
        /// Gets or sets the category of the API endpoint configuration.
        /// </summary>
        [JsonPropertyName("category")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public APIEndpointCategory? Category { get; set; }

        /// <summary>
        /// Gets or sets the subcategory of the API endpoint configuration.
        /// </summary>
        [JsonPropertyName("subcategory")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public APIEndpointSubcategory? Subcategory { get; set; }
    }
}
