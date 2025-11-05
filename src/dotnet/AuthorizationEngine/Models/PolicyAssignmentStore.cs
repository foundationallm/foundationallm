using FoundationaLLM.Common.Models.Authorization;
using System.Text.Json.Serialization;

namespace FoundationaLLM.AuthorizationEngine.Models
{
    /// <summary>
    /// Models the content of the policy assignments store managed by the FoundationaLLM.Authorization resource provider.
    /// </summary>
    public class PolicyAssignmentStore
    {
        /// <summary>
        /// The unique identifier of the FoundationaLLM instance.
        /// </summary>
        [JsonPropertyName("instance_id")]
        public required string InstanceId { get; set; }

        /// <summary>
        /// The list of all policy assignments in the FoundationaLLM instance.
        /// </summary>
        [JsonPropertyName("policy_assignments")]
        public required List<PolicyAssignment> PolicyAssignments { get; set; } = [];

        [JsonIgnore]
        public List<PolicyAssignment> InvalidPolicyAssignments { get; set; } = [];

        [JsonIgnore]
        public Dictionary<string, string> ValidationErrors { get; set; } = [];

        /// <summary>
        /// Loads calculated properties for all policy assignments.
        /// </summary>
        public void EnrichPolicyAssignments()
        {
            var allowedInstanceIds = new List<string>() { InstanceId };

            foreach (var policyAssignment in PolicyAssignments)
                try
                {
                    policyAssignment.Enrich(allowedInstanceIds);
                }
                catch (Exception ex)
                {
                    // If the scope cannot be parsed, we consider the policy assignment invalid
                    InvalidPolicyAssignments.Add(policyAssignment);
                    ValidationErrors.Add(policyAssignment.Name, ex.Message);
                }

            foreach (var invalidPolicyAssignment in InvalidPolicyAssignments)
                PolicyAssignments.Remove(invalidPolicyAssignment);
        }
    }
}
