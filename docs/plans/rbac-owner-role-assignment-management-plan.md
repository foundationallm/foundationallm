# Implementation Plan: Enable Owner Role to Manage Role Assignments on Scoped Resources

## Overview

### Problem Statement
Currently, users assigned the Owner role on a specific resource (e.g., an agent) cannot manage role assignments scoped to that resource. This limitation prevents resource owners from delegating access to their resources, which is a critical capability for proper access management.

**Example Scenario:**
- User A is assigned the Owner role on Agent X (`scope: /instances/{id}/providers/FoundationaLLM.Agent/agents/agent-x`)
- User A should be able to assign roles to others on Agent X (e.g., make User B a Reader of Agent X)
- Currently, User A cannot create these scoped role assignments because the authorization check fails

### Success Criteria
1. A user with the Owner role on a resource can create role assignments scoped to that resource
2. A user with the Owner role on a resource can delete role assignments scoped to that resource
3. A user with the Owner role on a resource can read role assignments scoped to that resource
4. The solution maintains security by preventing unauthorized scope elevation
5. Existing role assignment functionality continues to work without regression

### Users and Use Cases
- **Resource Owners**: Users who create or are assigned ownership of specific resources and need to delegate access
- **System Administrators**: Users with global Owner role who manage all resources
- **Team Leads**: Users who need to manage access to team-specific resources

## Technical Approach

### Current Architecture Analysis

#### Role Definitions
The Owner role is defined in `src/dotnet/Common/Templates/RoleDefinitions.cs`:
```csharp
{
    "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
    new RoleDefinition
    {
        DisplayName = "Owner",
        Description = "Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.",
        Permissions = [
            new RoleDefinitionPermissions
            {
                Actions = ["*"],
                NotActions = [],
                DataActions = [],
                NotDataActions = [],
            }
        ],
        ...
    }
}
```

The `Actions = ["*"]` wildcard should theoretically grant all permissions, including role assignment management.

#### Role Assignment Authorization Requirements
Role assignments are defined in `src/dotnet/Common/Constants/ResourceProviders/AuthorizationResourceProviderMetadata.cs`:
```csharp
{
    AuthorizationResourceTypeNames.RoleAssignments,
    new ResourceTypeDescriptor(AuthorizationResourceTypeNames.RoleAssignments, typeof(RoleAssignment))
    {
        AllowedTypes = [
            new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Write, ...),
            new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, ...)
        ],
        ...
    }
}
```

This translates to requiring:
- `FoundationaLLM.Authorization/roleAssignments/write` for creating role assignments
- `FoundationaLLM.Authorization/roleAssignments/delete` for deleting role assignments

#### Authorization Flow
1. **Request Flow**: Management API → Resource Provider → Authorization Check → Action Execution
2. **Authorization Check** (`ResourceProviderServiceBase.Authorize`):
   - Constructs authorizable action: `{providerName}/{resourceType}/{operation}`
   - Uses the **role assignment's resource path** for authorization
   - Example: `/instances/{id}/providers/FoundationaLLM.Authorization/roleAssignments/{raId}`

#### Root Cause
The authorization check for role assignment operations uses the role assignment's own resource path instead of checking permissions on the **scope** (target resource) of the role assignment. This means:

- When creating a role assignment with `scope: /instances/{id}/providers/FoundationaLLM.Agent/agents/agent-x`
- The authorization check evaluates permissions on `/instances/{id}/providers/FoundationaLLM.Authorization/roleAssignments/{raId}`
- But the user only has Owner role on `/instances/{id}/providers/FoundationaLLM.Agent/agents/agent-x`
- The authorization fails because the scopes don't match

### Proposed Solution

#### Option 1: Scope-Based Authorization Check (Recommended)
Modify the authorization logic for role assignment operations to check permissions on the **scope** of the role assignment instead of the role assignment resource itself.

**Implementation:**
1. Override authorization behavior in `AuthorizationResourceProviderService`
2. Extract the `scope` from the role assignment request
3. Perform authorization check against the scope resource path
4. Verify the user has the required permissions (`FoundationaLLM.Authorization/roleAssignments/*`) on the scope

**Advantages:**
- Aligns with Azure RBAC model where role assignment permissions are evaluated at the scope
- Maintains security by ensuring users can only assign roles within their permission boundary
- Minimal changes to existing code
- Clear and logical authorization model

**Security Considerations:**
- Prevent scope elevation: User cannot assign roles at a higher scope than they have permissions
- Validate that the scope exists and is a valid resource path
- Ensure the role being assigned is appropriate for the scope

#### Option 2: Hierarchical Permission Inheritance
Implement permission inheritance where role assignments automatically inherit authorization from their scope.

**Implementation:**
1. Modify `AuthorizationCore.ProcessAuthorizationRequestForResourcePath` to check parent scopes
2. When checking permissions on a role assignment, also check the scope resource
3. Cache the scope-based permissions for performance

**Advantages:**
- More generalized solution that could benefit other scenarios
- Follows principle of hierarchical access control

**Disadvantages:**
- More complex implementation
- Potential performance impact
- May require significant refactoring

### Recommended Approach: Option 1 (Scope-Based Authorization Check)

## Implementation Plan

### Phase 1: Foundation and Analysis (1-2 days)

#### Task 1.1: Create Custom Authorization Override in AuthorizationResourceProviderService (Medium)
**Dependencies:** None  
**Description:** Override the base `UpsertResourceAsync` and `DeleteResourceAsync` methods to implement custom authorization logic.

**Files to Modify:**
- `src/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderService.cs`

**Approach:**
1. Add a private method `AuthorizeRoleAssignmentOperation` that:
   - Extracts the scope from the role assignment request
   - Validates the scope is a valid resource path
   - Performs authorization check against the scope instead of the role assignment path
   - Ensures user has permission to assign roles at that scope
2. Call this method before executing role assignment operations

#### Task 1.2: Add Scope Validation (Small)
**Dependencies:** Task 1.1  
**Description:** Implement validation to ensure the scope in a role assignment request is valid.

**Files to Modify:**
- `src/dotnet/Authorization/Validation/RoleAssignmentValidator.cs`

**Approach:**
1. Add validation to check that scope is a valid resource path format
2. Validate that the scope resource type supports role assignments
3. Add validation to prevent scope elevation attacks

### Phase 2: Core Implementation (2-3 days)

#### Task 2.1: Implement Scope-Based Authorization Check (Large)
**Dependencies:** Task 1.1, Task 1.2  
**Description:** Implement the core authorization logic that checks permissions on the scope.

**Files to Modify:**
- `src/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderService.cs`
- `src/dotnet/Common/Services/ResourceProviders/ResourceProviderServiceBase.cs` (may need minor changes)

**Implementation Details:**
```csharp
private async Task<ResourcePathAuthorizationResult> AuthorizeRoleAssignmentOperation(
    string scope,
    string operation,
    UnifiedUserIdentity userIdentity)
{
    // Parse the scope to get the resource path
    var scopeResourcePath = new ResourcePath(scope, ...);
    
    // Build the authorization action for the scope resource
    // e.g., "FoundationaLLM.Agent/agents/write" if assigning roles to an agent
    var authorizableAction = $"FoundationaLLM.Authorization/{AuthorizationResourceTypeNames.RoleAssignments}/{operation}";
    
    // Check if user has permission on the scope
    var authResult = await _authorizationServiceClient.ProcessAuthorizationRequest(
        _instanceSettings.Id,
        authorizableAction,
        null,
        [scope],
        false,
        false,
        false,
        userIdentity);
    
    return authResult.AuthorizationResults[scope];
}
```

#### Task 2.2: Update Role Assignment Creation Flow (Medium)
**Dependencies:** Task 2.1  
**Description:** Modify the role assignment creation to use scope-based authorization.

**Files to Modify:**
- `src/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderService.cs`

**Approach:**
1. In `UpdateRoleAssignments` method, extract scope from role assignment
2. Call `AuthorizeRoleAssignmentOperation` with the scope
3. Only proceed with creation if authorized
4. Add appropriate error messages for authorization failures

#### Task 2.3: Update Role Assignment Deletion Flow (Medium)
**Dependencies:** Task 2.1  
**Description:** Modify the role assignment deletion to use scope-based authorization.

**Files to Modify:**
- `src/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderService.cs`

**Approach:**
1. In `DeleteResourceAsync` method for role assignments:
   - Retrieve the existing role assignment to get its scope
   - Call `AuthorizeRoleAssignmentOperation` with the scope
   - Only proceed with deletion if authorized

#### Task 2.4: Update Role Assignment Query/Read Flow (Small)
**Dependencies:** Task 2.1  
**Description:** Ensure role assignment queries respect scope-based permissions.

**Files to Modify:**
- `src/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderService.cs`

**Approach:**
1. Filter query results to only show role assignments where user has read permissions on the scope
2. This may already be handled by existing authorization, but needs verification

### Phase 3: Testing and Validation (2-3 days)

#### Task 3.1: Create Unit Tests (Large)
**Dependencies:** Task 2.1, Task 2.2, Task 2.3  
**Description:** Create comprehensive unit tests for scope-based authorization.

**Files to Create:**
- `tests/dotnet/Authorization/ResourceProviders/AuthorizationResourceProviderServiceTests.cs` (or extend existing)

**Test Cases:**
1. Owner on resource can create role assignment scoped to that resource
2. Owner on resource can delete role assignment scoped to that resource
3. Owner on resource cannot create role assignment scoped to different resource
4. Owner on resource cannot create role assignment scoped to parent resource (scope elevation prevention)
5. Non-owner cannot create role assignments even with valid scope
6. Global Owner can still create role assignments at any scope
7. Validation of invalid scopes fails appropriately

#### Task 3.2: Create Integration Tests (Medium)
**Dependencies:** Task 3.1  
**Description:** Create integration tests that validate the end-to-end flow.

**Files to Create/Modify:**
- Integration test files in `tests/dotnet/Integration/`

**Test Scenarios:**
1. Full workflow: Create agent → Assign owner → Owner creates role assignment → Verify access
2. Cross-resource validation: Cannot assign roles on resources where not owner
3. Scope hierarchy: Role assignments at different scope levels work correctly

#### Task 3.3: Manual Testing (Small)
**Dependencies:** Task 3.1, Task 3.2  
**Description:** Manual testing with real user scenarios.

**Test Scenarios:**
1. User A creates agent X
2. User A is automatically made owner of agent X
3. User A assigns Reader role to User B on agent X
4. User B can now read agent X
5. User A revokes User B's Reader role on agent X
6. User B can no longer read agent X

### Phase 4: Documentation and Deployment (1 day)

#### Task 4.1: Update Documentation (Small)
**Dependencies:** All previous tasks  
**Description:** Update RBAC documentation to explain scope-based role assignment authorization.

**Files to Update:**
- Create or update RBAC documentation file
- Update API documentation if needed

#### Task 4.2: Security Review (Medium)
**Dependencies:** All previous tasks  
**Description:** Conduct security review to ensure no permission escalation vulnerabilities.

**Security Checklist:**
- [ ] Verify scope elevation prevention works correctly
- [ ] Confirm wildcards in scopes are handled safely
- [ ] Test boundary conditions (root scope, invalid scopes, etc.)
- [ ] Verify authorization cache invalidation works correctly
- [ ] Test with various role combinations

#### Task 4.3: Code Review and Refinement (Small)
**Dependencies:** Task 4.2  
**Description:** Address code review feedback and refine implementation.

## Considerations

### Assumptions
1. The existing RBAC infrastructure is working correctly for other resource types
2. The `*` wildcard in Owner role permissions should match all actions including role assignments
3. Role assignments will continue to be stored and managed by the Authorization resource provider
4. The scope field in role assignments accurately represents the target resource

### Constraints
1. Must maintain backward compatibility with existing role assignments
2. Cannot break existing authorization flows for other resource types
3. Performance impact should be minimal (consider caching where appropriate)
4. Changes should align with Azure RBAC model for consistency

### Risks and Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Permission escalation vulnerability | High | Medium | Comprehensive security testing, scope validation, peer review |
| Performance degradation | Medium | Low | Implement authorization caching, optimize scope lookup |
| Breaking existing functionality | High | Low | Comprehensive unit and integration tests, phased rollout |
| Complexity in scope hierarchy | Medium | Medium | Start with simple flat scopes, document clearly |
| Cache invalidation issues | Medium | Medium | Ensure proper cache clearing on role assignment changes |

### Performance Considerations
1. **Authorization Cache**: Leverage existing authorization cache to avoid repeated checks
2. **Scope Lookup**: Optimize scope resource path parsing and validation
3. **Batch Operations**: Consider impact on bulk role assignment operations

### Security Considerations
1. **Scope Validation**: Strictly validate scope format and existence
2. **Scope Elevation Prevention**: Users cannot assign roles at scopes higher than their permission level
3. **Role Validation**: Ensure assigned roles are appropriate for the resource type
4. **Audit Logging**: Log all role assignment operations with scope information

## Not Included (Future Enhancements)

### Phase 2 Features (Not in Scope)
1. **Wildcard Scopes**: Support for wildcard patterns in scope (e.g., `/instances/*/providers/FoundationaLLM.Agent/*`)
2. **Delegated Administration**: Ability to delegate role assignment permissions without granting full Owner role
3. **Custom Roles**: Allowing resource owners to define custom roles scoped to their resources
4. **Role Assignment History**: Tracking and auditing complete history of role assignments
5. **Bulk Role Operations**: UI and API support for bulk role assignment operations
6. **Role Assignment Templates**: Pre-defined role assignment templates for common scenarios

### Known Limitations
1. This implementation focuses on RBAC (role-based access control) and does not address PBAC (policy-based access control) role assignments
2. Cross-instance role assignments are not covered in this plan
3. Service principal role assignments may need additional consideration
4. Role assignment notifications/alerts not included

## Alternative Approaches Considered

### Alternative 1: Add Explicit Role Assignment Actions to Owner Role
Instead of scope-based authorization, explicitly add role assignment actions to the Owner role definition.

**Why Not Chosen:**
- Owner role already has `*` wildcard which should include these actions
- Doesn't solve the core problem of scope mismatch
- Less flexible and maintainable

### Alternative 2: New "Delegated Owner" Role
Create a separate role specifically for managing role assignments at a scope.

**Why Not Chosen:**
- Adds unnecessary complexity
- Owner role should inherently have these permissions
- Doesn't align with industry standards (Azure, AWS, etc.)

### Alternative 3: Allow Role Assignment CRUD through Parent Resource Provider
Allow users to manage role assignments through the resource provider of the scoped resource (e.g., Agent resource provider handles role assignments for agents).

**Why Not Chosen:**
- Violates separation of concerns
- Would require changes to every resource provider
- More complex and harder to maintain

## Implementation Timeline

**Total Estimated Time: 6-9 days**

| Phase | Duration | Tasks |
|-------|----------|-------|
| Phase 1: Foundation | 1-2 days | Custom authorization override, scope validation |
| Phase 2: Core Implementation | 2-3 days | Scope-based auth, update CRUD flows |
| Phase 3: Testing | 2-3 days | Unit tests, integration tests, manual testing |
| Phase 4: Documentation | 1 day | Docs, security review, code review |

## Success Metrics

1. **Functionality**: All test cases pass, including edge cases
2. **Security**: No permission escalation vulnerabilities identified
3. **Performance**: Authorization checks complete within existing SLA (<100ms additional overhead)
4. **Compatibility**: No regression in existing role assignment functionality
5. **User Experience**: Clear error messages when authorization fails

## Rollout Plan

### Phase 1: Development and Testing (Weeks 1-2)
- Complete implementation
- Unit and integration testing
- Internal testing with dev environment

### Phase 2: Staging Validation (Week 3)
- Deploy to staging environment
- User acceptance testing
- Performance validation
- Security review

### Phase 3: Production Rollout (Week 4)
- Deploy to production with monitoring
- Monitor error rates and performance
- Gather user feedback
- Be prepared to rollback if issues arise

## Rollback Plan

If critical issues are discovered after deployment:

1. **Immediate**: Revert the changes to `AuthorizationResourceProviderService`
2. **Short-term**: Role assignments will fall back to requiring global permissions
3. **Communication**: Notify users of the temporary limitation
4. **Resolution**: Fix identified issues in development environment
5. **Re-deployment**: Follow rollout plan again after fixes

## Conclusion

This plan provides a comprehensive approach to enabling Owner role holders to manage role assignments on their scoped resources. The scope-based authorization check aligns with industry standards and provides a secure, maintainable solution. By following the phased implementation plan, we can deliver this critical functionality while maintaining system security and stability.
