using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.Skill;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides endpoints for User Portal skill review and management.
    /// Skills are part of the procedural memory feature for Code Interpreter.
    /// Skills are stored in Cosmos DB and scoped to agent-user combinations.
    /// </summary>
    /// <param name="cosmosDBService">The Cosmos DB service for skill storage.</param>
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
        IAzureCosmosDBService cosmosDBService,
        ICallContext callContext,
        ILogger<SkillsController> logger) : ControllerBase
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<SkillsController> _logger = logger;

        /// <summary>
        /// Gets a skill by its identifier for user review.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier.</param>
        /// <returns>The skill details.</returns>
        [HttpGet("{skillId}", Name = "GetSkill")]
        public async Task<IActionResult> GetSkill(string instanceId, string skillId)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                var skill = await _cosmosDBService.GetSkillAsync(upn, skillId);

                if (skill == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                // Verify the user owns this skill
                if (!string.Equals(skill.UPN, upn, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("You do not have permission to view this skill.");
                }

                return Ok(skill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while retrieving the skill.");
            }
        }

        /// <summary>
        /// Gets all skills owned by the current user, optionally filtered by agent.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="agentObjectId">Optional agent object ID to filter skills by.</param>
        /// <returns>List of skills owned by the user.</returns>
        [HttpGet(Name = "GetUserSkills")]
        public async Task<IActionResult> GetUserSkills(string instanceId, [FromQuery] string? agentObjectId = null)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                var skills = await _cosmosDBService.GetSkillsAsync(upn, agentObjectId);

                return Ok(skills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user skills");
                return StatusCode(500, "An error occurred while retrieving skills.");
            }
        }

        /// <summary>
        /// Creates or updates a skill.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skill">The skill to create or update.</param>
        /// <returns>The created or updated skill.</returns>
        [HttpPost(Name = "UpsertSkill")]
        public async Task<IActionResult> UpsertSkill(string instanceId, [FromBody] SkillReference skill)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                // Ensure the skill is owned by the current user
                skill.UPN = upn;
                
                // Set timestamps
                var now = DateTimeOffset.UtcNow;
                if (skill.CreatedOn == default)
                {
                    skill.CreatedOn = now;
                }
                skill.UpdatedOn = now;

                var result = await _cosmosDBService.UpsertSkillAsync(skill);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting skill {SkillName}", skill.Name);
                return StatusCode(500, "An error occurred while saving the skill.");
            }
        }

        /// <summary>
        /// Approves a skill that is pending approval, making it active and available for use.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier.</param>
        /// <returns>Result of the approval operation.</returns>
        [HttpPost("{skillId}/approve", Name = "ApproveSkill")]
        public async Task<IActionResult> ApproveSkill(string instanceId, string skillId)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                var skill = await _cosmosDBService.GetSkillAsync(upn, skillId);

                if (skill == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                if (!string.Equals(skill.UPN, upn, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("You do not have permission to approve this skill.");
                }

                if (skill.Status != SkillStatus.PendingApproval)
                {
                    return BadRequest($"Skill '{skillId}' is not pending approval. Current status: {skill.Status}");
                }

                // Update the skill status
                skill.Status = SkillStatus.Active;
                skill.UpdatedOn = DateTimeOffset.UtcNow;

                await _cosmosDBService.UpsertSkillAsync(skill);

                return Ok(new { message = $"Skill '{skill.Name}' has been approved and is now active." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while approving the skill.");
            }
        }

        /// <summary>
        /// Rejects (deletes) a skill, removing it from storage.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier.</param>
        /// <returns>Result of the delete operation.</returns>
        [HttpDelete("{skillId}", Name = "RejectSkill")]
        public async Task<IActionResult> RejectSkill(string instanceId, string skillId)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                var skill = await _cosmosDBService.GetSkillAsync(upn, skillId);

                if (skill == null)
                {
                    return NotFound($"Skill '{skillId}' not found.");
                }

                if (!string.Equals(skill.UPN, upn, StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid("You do not have permission to reject this skill.");
                }

                await _cosmosDBService.DeleteSkillAsync(skill);

                return Ok(new { message = $"Skill '{skill.Name}' has been rejected and removed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while rejecting the skill.");
            }
        }

        /// <summary>
        /// Updates execution statistics for a skill after it has been used.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="skillId">The skill identifier.</param>
        /// <param name="request">The execution result.</param>
        /// <returns>The updated skill.</returns>
        [HttpPost("{skillId}/execute", Name = "RecordSkillExecution")]
        public async Task<IActionResult> RecordSkillExecution(
            string instanceId, 
            string skillId, 
            [FromBody] SkillExecutionRequest request)
        {
            try
            {
                var upn = _callContext.CurrentUserIdentity?.UPN;
                if (string.IsNullOrEmpty(upn))
                {
                    return Unauthorized("User identity not found.");
                }

                var skill = await _cosmosDBService.UpdateSkillExecutionAsync(upn, skillId, request.Success);

                return Ok(skill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording skill execution for {SkillId}", skillId);
                return StatusCode(500, "An error occurred while recording the skill execution.");
            }
        }
    }

    /// <summary>
    /// Request model for recording a skill execution.
    /// </summary>
    public class SkillExecutionRequest
    {
        /// <summary>
        /// Whether the skill execution was successful.
        /// </summary>
        public bool Success { get; set; }
    }
}
