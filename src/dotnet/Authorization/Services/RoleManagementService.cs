using FoundationaLLM.Authorization.Interfaces;
using FoundationaLLM.Authorization.Models;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Implements the <see cref="IRoleManagementService"/> interface.
    /// </summary>
    public class RoleManagementService : IRoleManagementService
    {
        private readonly ILogger<RoleManagementService> _logger;
        /// <summary>
        /// Constructor for the role management service.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public RoleManagementService(
            ILogger<RoleManagementService> logger) => _logger = logger;

        /// <inheritdoc/>
        public Task<RoleAssignment> GetRoleAssignment(string instanceId, Guid roleDefinitionId, Guid roleAssignmentId) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<RoleAssignment>> GetRoleAssignments(string instanceId, Guid roleDefinitionId) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<RoleAssignment>> GetRoleAssignmentsForScope(string instanceId, string scope) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<RoleDefinition> GetRoleDefinition(string instanceId, Guid roleDefinitionId) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<RoleDefinition>> GetRoleDefinitions(string instanceId) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<RoleDefinition>> GetRoleDefinitionsForScope(string instanceId, string scope) => throw new NotImplementedException();
    }
}
