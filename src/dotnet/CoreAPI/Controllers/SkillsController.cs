using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Skill;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides endpoints for User Portal skill review and management.
    /// Skills are part of the procedural memory feature for Code Interpreter.
    /// </summary>
    /// <param name="resourceProviderServices">The collection of resource provider services.</param>
    /// <param name="callContext">The call context containing user identity.</param>
    /// <param name="logger">The logging interface.</param>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [Authorize(
        AuthenticationSchemes = AgentAccessTokenDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.FoundationaLLMAgentAccessToken)]
    [ApiController]
    [Route("instances/{instanceId}/skills")]
    public class SkillsController(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ICallContext callContext,
        ILogger<SkillsController> logger) : ControllerBase
    {
        private readonly IResourceProviderService? _skillResourceProvider = 
            resourceProviderServices.SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Skill);
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<SkillsController> _logger = logger;

        /// <summary>
        /// Gets a skill by its object ID for user review.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier (name).</param>
        /// <returns>The skill details.</returns>
        [HttpGet("{skillId}", Name = "GetSkill")]
        public async Task<IActionResult> GetSkill(string instanceId, string skillId)
        {
            if (_skillResourceProvider == null)
            {
                return StatusCode(503, "Skill resource provider is not available.");
            }

            try
            {
                var userIdentity = _callContext.CurrentUserIdentity ?? new UnifiedUserIdentity
                {
                    Name = "Unknown",
                    UPN = "unknown@unknown.com"
                };

                var result = await _skillResourceProvider.GetResource<Skill>(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}/{skillId}",
                    userIdentity);

                if (result == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                // Only return the skill if the user owns it
                if (result.OwnerUserId != userIdentity.UPN)
                {
                    return Forbid("You do not have permission to view this skill.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while retrieving the skill.");
            }
        }

        /// <summary>
        /// Approves a skill that is pending approval, making it active and available for use.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier (name).</param>
        /// <returns>Result of the approval operation.</returns>
        [HttpPost("{skillId}/approve", Name = "ApproveSkill")]
        public async Task<IActionResult> ApproveSkill(string instanceId, string skillId)
        {
            if (_skillResourceProvider == null)
            {
                return StatusCode(503, "Skill resource provider is not available.");
            }

            try
            {
                var userIdentity = _callContext.CurrentUserIdentity ?? new UnifiedUserIdentity
                {
                    Name = "Unknown",
                    UPN = "unknown@unknown.com"
                };

                // First, verify the user owns this skill
                var skill = await _skillResourceProvider.GetResource<Skill>(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}/{skillId}",
                    userIdentity);

                if (skill == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                if (skill.OwnerUserId != userIdentity.UPN)
                {
                    return Forbid("You do not have permission to approve this skill.");
                }

                // Execute the approve action
                var result = await _skillResourceProvider.ExecuteActionAsync(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}/{skillId}/{SkillResourceProviderActions.Approve}",
                    string.Empty,
                    userIdentity);

                return Ok(new { message = $"Skill '{skillId}' has been approved and is now active." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while approving the skill.");
            }
        }

        /// <summary>
        /// Rejects (deletes) a skill, removing it from storage.
        /// This can be used for skills that are pending approval or already active.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier (name).</param>
        /// <returns>Result of the delete operation.</returns>
        [HttpDelete("{skillId}", Name = "RejectSkill")]
        public async Task<IActionResult> RejectSkill(string instanceId, string skillId)
        {
            if (_skillResourceProvider == null)
            {
                return StatusCode(503, "Skill resource provider is not available.");
            }

            try
            {
                var userIdentity = _callContext.CurrentUserIdentity ?? new UnifiedUserIdentity
                {
                    Name = "Unknown",
                    UPN = "unknown@unknown.com"
                };

                // First, verify the user owns this skill
                var skill = await _skillResourceProvider.GetResource<Skill>(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}/{skillId}",
                    userIdentity);

                if (skill == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                if (skill.OwnerUserId != userIdentity.UPN)
                {
                    return Forbid("You do not have permission to reject this skill.");
                }

                // Delete the skill
                await _skillResourceProvider.DeleteResourceAsync(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}/{skillId}",
                    userIdentity);

                return Ok(new { message = $"Skill '{skillId}' has been rejected and removed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while rejecting the skill.");
            }
        }

        /// <summary>
        /// Gets all skills owned by the current user for a specific agent.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentObjectId">The agent object ID to filter skills by.</param>
        /// <returns>List of skills owned by the user for the specified agent.</returns>
        [HttpGet(Name = "GetUserSkills")]
        public async Task<IActionResult> GetUserSkills(string instanceId, [FromQuery] string? agentObjectId = null)
        {
            if (_skillResourceProvider == null)
            {
                return StatusCode(503, "Skill resource provider is not available.");
            }

            try
            {
                var userIdentity = _callContext.CurrentUserIdentity ?? new UnifiedUserIdentity
                {
                    Name = "Unknown",
                    UPN = "unknown@unknown.com"
                };

                // Get all skills from the resource provider
                var allSkills = await _skillResourceProvider.GetResourcesAsync<Skill>(
                    $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Skill}/{SkillResourceTypeNames.Skills}",
                    userIdentity);

                // Filter to only the current user's skills
                var userSkills = allSkills
                    .Where(s => s.OwnerUserId == userIdentity.UPN)
                    .Where(s => string.IsNullOrEmpty(agentObjectId) || s.OwnerAgentObjectId == agentObjectId)
                    .ToList();

                return Ok(userSkills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user skills");
                return StatusCode(500, "An error occurred while retrieving skills.");
            }
        }
    }
}
