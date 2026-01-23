using FoundationaLLM.Authorization.ResourceProviders;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundationaLLM.Common.Tests.ResourceProviders
{
    /// <summary>
    /// Unit tests for AuthorizationResourceProviderService focusing on scope-based authorization.
    /// </summary>
    public class AuthorizationResourceProviderServiceTests
    {
        private readonly IOptions<InstanceSettings> _instanceOptions;
        private readonly IOptions<ResourceProviderCacheSettings> _cacheOptions;
        private readonly IAuthorizationServiceClient _authorizationServiceClient;
        private readonly IIdentityManagementService _identityManagementService;
        private readonly IResourceValidatorFactory _resourceValidatorFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly UnifiedUserIdentity _userIdentity;
        private readonly string _instanceId = "test-instance";

        public AuthorizationResourceProviderServiceTests()
        {
            // Setup common mocks
            var instanceSettings = new InstanceSettings { Id = _instanceId };
            _instanceOptions = Options.Create(instanceSettings);

            var cacheSettings = new ResourceProviderCacheSettings();
            _cacheOptions = Options.Create(cacheSettings);

            _authorizationServiceClient = Substitute.For<IAuthorizationServiceClient>();
            _identityManagementService = Substitute.For<IIdentityManagementService>();
            _resourceValidatorFactory = Substitute.For<IResourceValidatorFactory>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _loggerFactory = Substitute.For<ILoggerFactory>();
            _loggerFactory.CreateLogger<AuthorizationResourceProviderService>()
                .Returns(Substitute.For<ILogger<AuthorizationResourceProviderService>>());

            _userIdentity = new UnifiedUserIdentity
            {
                UserId = "user-123",
                UPN = "user@example.com",
                Username = "testuser",
                Name = "Test User",
                GroupIds = []
            };
        }

        [Fact]
        public async Task UpdateRoleAssignments_WithOwnerRoleOnScope_ShouldAuthorizeAndSucceed()
        {
            // Arrange
            var agentScope = $"/instances/{_instanceId}/providers/FoundationaLLM.Agent/agents/test-agent";
            var roleAssignmentName = Guid.NewGuid().ToString();
            var roleAssignment = new RoleAssignment
            {
                Name = roleAssignmentName,
                Description = "Test role assignment",
                PrincipalId = "user-456",
                PrincipalType = "User",
                RoleDefinitionId = RoleDefinitionIds.Reader,
                Scope = agentScope
            };

            var serializedRoleAssignment = System.Text.Json.JsonSerializer.Serialize(roleAssignment);

            // Setup authorization to succeed on the scope
            var authResult = new ActionAuthorizationResult
            {
                AuthorizationResults = new Dictionary<string, ResourcePathAuthorizationResult>
                {
                    [agentScope] = new ResourcePathAuthorizationResult
                    {
                        ResourcePath = agentScope,
                        Authorized = true
                    }
                }
            };

            _authorizationServiceClient
                .ProcessAuthorizationRequest(
                    _instanceId,
                    $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Write}",
                    null,
                    Arg.Is<List<string>>(list => list.Contains(agentScope)),
                    false,
                    false,
                    false,
                    _userIdentity)
                .Returns(authResult);

            _authorizationServiceClient
                .CreateRoleAssignment(_instanceId, Arg.Any<RoleAssignmentCreateRequest>(), _userIdentity)
                .Returns(new RoleAssignmentOperationResult { Success = true });

            var validator = Substitute.For<FluentValidation.IValidator<RoleAssignment>>();
            validator.ValidateAsync(Arg.Any<FluentValidation.ValidationContext<RoleAssignment>>(), Arg.Any<CancellationToken>())
                .Returns(new FluentValidation.Results.ValidationResult());
            _resourceValidatorFactory.GetValidator<RoleAssignment>().Returns(validator);

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act
            var result = await service.HandlePostAsync(resourcePath, serializedRoleAssignment, null, _userIdentity);

            // Assert
            Assert.NotNull(result);
            await _authorizationServiceClient.Received(1).ProcessAuthorizationRequest(
                _instanceId,
                $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Write}",
                null,
                Arg.Is<List<string>>(list => list.Contains(agentScope)),
                false,
                false,
                false,
                _userIdentity);
        }

        [Fact]
        public async Task UpdateRoleAssignments_WithoutOwnerRoleOnScope_ShouldThrowAuthorizationException()
        {
            // Arrange
            var agentScope = $"/instances/{_instanceId}/providers/FoundationaLLM.Agent/agents/test-agent";
            var roleAssignmentName = Guid.NewGuid().ToString();
            var roleAssignment = new RoleAssignment
            {
                Name = roleAssignmentName,
                Description = "Test role assignment",
                PrincipalId = "user-456",
                PrincipalType = "User",
                RoleDefinitionId = RoleDefinitionIds.Reader,
                Scope = agentScope
            };

            var serializedRoleAssignment = System.Text.Json.JsonSerializer.Serialize(roleAssignment);

            // Setup authorization to fail on the scope
            var authResult = new ActionAuthorizationResult
            {
                AuthorizationResults = new Dictionary<string, ResourcePathAuthorizationResult>
                {
                    [agentScope] = new ResourcePathAuthorizationResult
                    {
                        ResourcePath = agentScope,
                        Authorized = false
                    }
                }
            };

            _authorizationServiceClient
                .ProcessAuthorizationRequest(
                    _instanceId,
                    $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Write}",
                    null,
                    Arg.Is<List<string>>(list => list.Contains(agentScope)),
                    false,
                    false,
                    false,
                    _userIdentity)
                .Returns(authResult);

            var validator = Substitute.For<FluentValidation.IValidator<RoleAssignment>>();
            validator.ValidateAsync(Arg.Any<FluentValidation.ValidationContext<RoleAssignment>>(), Arg.Any<CancellationToken>())
                .Returns(new FluentValidation.Results.ValidationResult());
            _resourceValidatorFactory.GetValidator<RoleAssignment>().Returns(validator);

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceProviderException>(
                async () => await service.HandlePostAsync(resourcePath, serializedRoleAssignment, null, _userIdentity));

            Assert.Contains("You do not have permission to manage role assignments", exception.Message);
            Assert.Equal(403, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteRoleAssignment_WithOwnerRoleOnScope_ShouldAuthorizeAndSucceed()
        {
            // Arrange
            var agentScope = $"/instances/{_instanceId}/providers/FoundationaLLM.Agent/agents/test-agent";
            var roleAssignmentName = Guid.NewGuid().ToString();
            var existingRoleAssignment = new RoleAssignment
            {
                Name = roleAssignmentName,
                Description = "Test role assignment",
                ObjectId = $"/instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                PrincipalId = "user-456",
                PrincipalType = "User",
                RoleDefinitionId = RoleDefinitionIds.Reader,
                Scope = agentScope
            };

            // Setup GetRoleAssignments to return the existing role assignment
            _authorizationServiceClient
                .GetRoleAssignments(_instanceId, Arg.Any<RoleAssignmentQueryParameters>(), _userIdentity)
                .Returns(new List<RoleAssignment> { existingRoleAssignment });

            // Setup authorization to succeed on the scope
            var authResult = new ActionAuthorizationResult
            {
                AuthorizationResults = new Dictionary<string, ResourcePathAuthorizationResult>
                {
                    [agentScope] = new ResourcePathAuthorizationResult
                    {
                        ResourcePath = agentScope,
                        Authorized = true
                    }
                }
            };

            _authorizationServiceClient
                .ProcessAuthorizationRequest(
                    _instanceId,
                    $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Delete}",
                    null,
                    Arg.Is<List<string>>(list => list.Contains(agentScope)),
                    false,
                    false,
                    false,
                    _userIdentity)
                .Returns(authResult);

            _authorizationServiceClient
                .DeleteRoleAssignment(_instanceId, roleAssignmentName, _userIdentity)
                .Returns(new RoleAssignmentOperationResult { Success = true });

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act
            await service.HandleDeleteAsync(resourcePath, _userIdentity);

            // Assert
            await _authorizationServiceClient.Received(1).ProcessAuthorizationRequest(
                _instanceId,
                $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Delete}",
                null,
                Arg.Is<List<string>>(list => list.Contains(agentScope)),
                false,
                false,
                false,
                _userIdentity);

            await _authorizationServiceClient.Received(1).DeleteRoleAssignment(_instanceId, roleAssignmentName, _userIdentity);
        }

        [Fact]
        public async Task DeleteRoleAssignment_WithoutOwnerRoleOnScope_ShouldThrowAuthorizationException()
        {
            // Arrange
            var agentScope = $"/instances/{_instanceId}/providers/FoundationaLLM.Agent/agents/test-agent";
            var roleAssignmentName = Guid.NewGuid().ToString();
            var existingRoleAssignment = new RoleAssignment
            {
                Name = roleAssignmentName,
                Description = "Test role assignment",
                ObjectId = $"/instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                PrincipalId = "user-456",
                PrincipalType = "User",
                RoleDefinitionId = RoleDefinitionIds.Reader,
                Scope = agentScope
            };

            // Setup GetRoleAssignments to return the existing role assignment
            _authorizationServiceClient
                .GetRoleAssignments(_instanceId, Arg.Any<RoleAssignmentQueryParameters>(), _userIdentity)
                .Returns(new List<RoleAssignment> { existingRoleAssignment });

            // Setup authorization to fail on the scope
            var authResult = new ActionAuthorizationResult
            {
                AuthorizationResults = new Dictionary<string, ResourcePathAuthorizationResult>
                {
                    [agentScope] = new ResourcePathAuthorizationResult
                    {
                        ResourcePath = agentScope,
                        Authorized = false
                    }
                }
            };

            _authorizationServiceClient
                .ProcessAuthorizationRequest(
                    _instanceId,
                    $"{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{AuthorizableOperations.Delete}",
                    null,
                    Arg.Is<List<string>>(list => list.Contains(agentScope)),
                    false,
                    false,
                    false,
                    _userIdentity)
                .Returns(authResult);

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceProviderException>(
                async () => await service.HandleDeleteAsync(resourcePath, _userIdentity));

            Assert.Contains("You do not have permission to manage role assignments", exception.Message);
            Assert.Equal(403, exception.StatusCode);

            // Verify delete was NOT called
            await _authorizationServiceClient.DidNotReceive().DeleteRoleAssignment(_instanceId, roleAssignmentName, _userIdentity);
        }

        [Fact]
        public async Task DeleteRoleAssignment_WhenRoleAssignmentNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var roleAssignmentName = Guid.NewGuid().ToString();

            // Setup GetRoleAssignments to return empty list
            _authorizationServiceClient
                .GetRoleAssignments(_instanceId, Arg.Any<RoleAssignmentQueryParameters>(), _userIdentity)
                .Returns(new List<RoleAssignment>());

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceProviderException>(
                async () => await service.HandleDeleteAsync(resourcePath, _userIdentity));

            Assert.Contains("Could not locate the role assignment", exception.Message);
            Assert.Equal(404, exception.StatusCode);
        }

        [Fact]
        public async Task UpdateRoleAssignments_WithEmptyScope_ShouldThrowBadRequestException()
        {
            // Arrange
            var roleAssignmentName = Guid.NewGuid().ToString();
            var roleAssignment = new RoleAssignment
            {
                Name = roleAssignmentName,
                Description = "Test role assignment",
                PrincipalId = "user-456",
                PrincipalType = "User",
                RoleDefinitionId = RoleDefinitionIds.Reader,
                Scope = "" // Empty scope
            };

            var serializedRoleAssignment = System.Text.Json.JsonSerializer.Serialize(roleAssignment);

            var validator = Substitute.For<FluentValidation.IValidator<RoleAssignment>>();
            validator.ValidateAsync(Arg.Any<FluentValidation.ValidationContext<RoleAssignment>>(), Arg.Any<CancellationToken>())
                .Returns(new FluentValidation.Results.ValidationResult());
            _resourceValidatorFactory.GetValidator<RoleAssignment>().Returns(validator);

            var service = new AuthorizationResourceProviderService(
                _instanceOptions,
                _cacheOptions,
                _authorizationServiceClient,
                _identityManagementService,
                _resourceValidatorFactory,
                _serviceProvider,
                _loggerFactory);

            // Wait for initialization to complete
            await service.InitializationTask;

            var resourcePath = $"instances/{_instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ResourceProviderException>(
                async () => await service.HandlePostAsync(resourcePath, serializedRoleAssignment, null, _userIdentity));

            Assert.Contains("scope cannot be empty", exception.Message);
            Assert.Equal(400, exception.StatusCode);
        }
    }
}
