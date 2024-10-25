using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.ContentSafety
{
    /// <summary>
    /// Text analysis results.
    /// </summary>
    public class AnalyzeTextResult
    {
        /// <summary> Analysis result for categories. </summary>
        [JsonPropertyName("categoriesAnalysis")]
        public required List<TextCategoryResult> CategoriesAnalysis { get; set; }
    }

    /// <summary>
    /// Text category results.
    /// </summary>
    public class TextCategoryResult
    {
        /// <summary> The text analysis category. </summary>
        [JsonPropertyName("category")]
        public required string Category { get; set; }

        /// <summary> The value increases with the severity of the input content. The value of this field is determined by the output type specified in the request. The output type could be ‘FourSeverityLevels’ or ‘EightSeverity Levels’, and the output value can be 0, 2, 4, 6 or 0, 1, 2, 3, 4, 5, 6, or 7. </summary>
        [JsonPropertyName("severity")]
        public int? Severity { get; set; }
    }

    /// <summary>
    /// Text category constants.
    /// </summary>
    public static class TextCategory
    {
        /// <summary>
        /// Hate.
        /// </summary>
        public const string Hate = "Hate";

        /// <summary>
        /// Violence.
        /// </summary>
        public const string Violence = "Violence";

        /// <summary>
        /// SelfHarm.
        /// </summary>
        public const string SelfHarm = "SelfHarm";

        /// <summary>
        /// Sexual.
        /// </summary>
        public const string Sexual = "Sexual";
    }
}
