using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the model for a data pipeline trigger.
    /// </summary>
    public class DataPipelineTrigger
    {
        /// <summary>
        /// Gets or sets the name of the data pipeline trigger.
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of trigger that initiates the execution of the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required DataPipelineTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the schedule of the trigger in Cron format.
        /// </summary>
        /// <remarks>
        /// This property is valid only when TriggerType = Schedule.
        /// </remarks>
        [JsonPropertyName("trigger_cron_schedule")]
        public string? TriggerCronSchedule { get; set; }

        /// <summary>
        /// Gets or sets a dictionary that contains the parameter values required to trigger the pipeline.
        /// </summary>
        /// <remarks>
        /// When the trigger type is Event or Schedule, the dictionary must contain parameter values
        /// for all the data pipeline parameters (which are the union of all the parameters required
        /// by the data source and the data pipeline stages).
        /// </remarks>
        [JsonPropertyName("parameter_values")]
        public required Dictionary<string, object> ParameterValues { get; set; } = [];
    }
}
