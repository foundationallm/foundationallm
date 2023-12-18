using FoundationaLLM.Vectorization.Exceptions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Represents a vectorization request.
    /// </summary>
    public class VectorizationRequest
    {
        /// <summary>
        /// The unique identifier of the vectorization request. Subsequent vectorization requests
        /// referring to the same content will have different unique identifiers.
        /// </summary>
        [JsonPropertyOrder(0)]
        [JsonPropertyName("id")]
        public required string Id { get;set; }

        /// <summary>
        /// The unique identifier of the content (i.e., document) being vectorized.
        /// </summary>
        [JsonPropertyOrder(1)]
        [JsonPropertyName("content_id")]
        public required string ContentId { get; set; }

        /// <summary>
        /// The list of vectorization steps requested by the vectorization request.
        /// Vectorization steps are identified by unique names like "extract", "partition", "embed", "index", etc.
        /// </summary>
        [JsonPropertyOrder(10)]
        [JsonPropertyName("steps")]
        public required List<VectorizationStep> Steps { get; set; }

        /// <summary>
        /// The ordered list of the names of the vectorization steps that were already completed.
        /// </summary>
        [JsonPropertyOrder(11)]
        [JsonPropertyName("completed_steps")]
        public required List<string> CompletedSteps { get; set; }

        /// <summary>
        /// The ordered list of the names of the vectorization steps that still need to be executed.
        /// </summary>
        [JsonPropertyOrder(12)]
        [JsonPropertyName("remaining_steps")]
        public required List<string> RemainingSteps { get; set; }

        /// <summary>
        /// Indicates whether the vectorization process is complete or not.
        /// </summary>
        [JsonIgnore]
        public bool Complete => this.RemainingSteps.Count == 0;

        /// <summary>
        /// Advances the vectorization pipeline to the next step, returning the name of the next step to execute.
        /// The name returned is used to choose the next request source to which the vectorization request will be added.
        /// </summary>
        public string MoveToNextStep()
        {
            if (this.RemainingSteps.Count == 0)
            {
                throw new VectorizationException("The list of remaining steps is empty");
            }

            var stepName = this.RemainingSteps.First();
            this.RemainingSteps.RemoveAt(0);
            this.CompletedSteps.Add(stepName);

            return stepName;
        }

        /// <summary>
        /// Reverts the vectorization pipeline to the previous step, returning the name of the step to execute
        /// </summary>
        public string RollbackToPreviousStep()
        {
            if (this.CompletedSteps.Count == 0)
            {
                throw new VectorizationException("The list of completed steps is empty");
            }

            var stepName = this.CompletedSteps.Last();
            this.CompletedSteps.RemoveAt(this.CompletedSteps.Count - 1);
            this.RemainingSteps.Insert(0, stepName);

            return stepName;
        }

        /// <summary>
        /// Gets the vectorization pipeline step that has a specific identifier.
        /// </summary>
        /// <param name="step">The identifier of the vectorization pipeline step.</param>
        /// <returns>An instances of the <see cref="VectorizationStep"/> class with the details required by the step handler.</returns>
        public VectorizationStep? this[string step]
        { get { return Steps.SingleOrDefault(s => s.Id == step); } }
    }
}
