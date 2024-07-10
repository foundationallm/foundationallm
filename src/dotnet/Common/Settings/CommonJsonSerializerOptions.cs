using System.Text.Json;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// JSON serializer settings for the API classes and their libraries.
    /// </summary>
    public static class CommonJsonSerializerOptions
    {
        /// <summary>
        /// Configures the System.Text.Json JSON serializer settings.
        /// </summary>
        /// <returns></returns>
        public static JsonSerializerOptions GetJsonSerializerOptions() =>
            new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        /// <summary>
        /// Configures the System.Text.Json JSON serializer settings.
        /// </summary>
        /// <param name="customizer">A function that customizes the default settings.</param>
        /// <returns></returns>
        public static JsonSerializerOptions GetJsonSerializerOptions(
            Func<JsonSerializerOptions, JsonSerializerOptions> customizer)
        {
            var options = GetJsonSerializerOptions();
            return customizer(options);
        }
    }
}
