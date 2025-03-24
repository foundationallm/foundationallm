using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Models.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context
{
    /// <summary>
    /// Represents a file record.
    /// </summary>
    public class ContextCodeSessionRecord : ContextRecord
    {
        /// <summary>
        /// Gets or sets the type of the context record.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-99)]
        public override string Type { get; set; } =
            ContextRecordTypeNames.CodeSessionRecord;

        /// <summary>
        /// Gets or sets the conversation identifier.
        /// </summary>
        [JsonPropertyName("conversation_id")]
        [JsonPropertyOrder(0)]
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the code session provider name.
        /// </summary>
        [JsonPropertyName("endopoint_provider")]
        [JsonPropertyOrder(1)]
        public string EndpointProvider { get; set; }

        /// <summary>
        /// Gets or sets the code session endpoint.
        /// </summary>
        [JsonPropertyName("endpoint")]
        [JsonPropertyOrder(2)]
        public string Endpoint { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCodeSessionRecord"/> class.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ContextCodeSessionRecord()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            // Required for deserialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextCodeSessionRecord"/> class.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="codeSessionId">The code session unique identifier.</param>
        /// <param name="endpointProvider">The code session provider.</param>
        /// <param name="endpoint">The code session provider endpoint.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing details about the user identity.</param>
        public ContextCodeSessionRecord(
            string instanceId,
            string conversationId,
            string codeSessionId,
            string endpointProvider,
            string endpoint,
            UnifiedUserIdentity userIdentity) : base(
                codeSessionId,
                instanceId,
                userIdentity)
        {
            ConversationId = conversationId;
            EndpointProvider = endpointProvider;
            Endpoint = endpoint;
        }
    }
}
