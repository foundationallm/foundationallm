# Implementation Plan: FoundationaLLM.Infrastructure Resource Provider

## Overview

### Problem Statement
We need to create a new resource provider named `FoundationaLLM.Infrastructure` to manage infrastructure components used by FoundationaLLM deployments. FoundationaLLM can be deployed in two ways:
1. Using Azure Container Apps Environment with multiple Azure Container Apps
2. Using Azure Kubernetes Service (AKS) with multiple Kubernetes deployments

### Success Criteria
- New resource provider is created following established patterns
- Support for reading Azure Container Apps Environments and Azure Kubernetes Services (read-only)
- Support for full CRUD operations on Azure Container Apps and Azure Kubernetes Service Deployments
- Ability to extract JSON definitions for existing services

### Users and Use Cases
- Platform administrators managing FoundationaLLM infrastructure
- DevOps engineers monitoring and scaling services
- Automation tools extracting infrastructure configurations

## Technical Approach

### High-Level Architecture
The implementation follows the established resource provider pattern in FoundationaLLM:

```
┌─────────────────────────────────────────────────────────────────┐
│                    FoundationaLLM.Infrastructure                │
├─────────────────────────────────────────────────────────────────┤
│  Resource Types:                                                │
│  ├── azureContainerAppsEnvironments (read-only)                 │
│  │   └── azureContainerApps (full CRUD, subordinate)           │
│  ├── azureKubernetesServices (read-only)                        │
│  │   └── azureKubernetesServiceDeployments (full CRUD, subord.)│
└─────────────────────────────────────────────────────────────────┘
```

### Key Technology Choices
- .NET 8 following the existing codebase patterns
- Extends `ResourceProviderServiceBase<TResourceReference>` 
- Uses Azure SDK for interacting with Azure resources
- Follows the existing authorization model

### Resource Types Structure

| Resource Type | Name | Read | Write | Delete | Actions |
|--------------|------|------|-------|--------|---------|
| Azure Container Apps Environment | `azureContainerAppsEnvironments` | ✅ | ❌ | ❌ | - |
| Azure Container App | `azureContainerApps` | ✅ | ✅ | ✅ | `restart`, `scale` |
| Azure Kubernetes Service | `azureKubernetesServices` | ✅ | ❌ | ❌ | - |
| AKS Deployment | `azureKubernetesServiceDeployments` | ✅ | ✅ | ✅ | `restart`, `scale` |

### Services to be Managed
The following services can be deployed as container apps or Kubernetes deployments:
- authorization-api
- context-api
- core-api
- core-job
- datapipeline-api
- datapipeline-frontendworker
- datapipeline-backendworker
- gatekeeper-api
- gatekeeper-integration-api
- gateway-adapter-api
- gateway-api
- langchain-api
- management-api
- orchestration-api
- semantic-kernel-api
- state-api

## Implementation Plan

### Phase 1: Constants and Shared Components (Common Project)

#### Task 1.1: Add Resource Provider Name
**File:** `src/dotnet/Common/Constants/ResourceProviders/ResourceProviderNames.cs`
**Complexity:** Small
**Changes:**
- Add `FoundationaLLM_Infrastructure` constant
- Add to `All` list

#### Task 1.2: Create Resource Type Names
**File:** `src/dotnet/Common/Constants/ResourceProviders/InfrastructureResourceTypeNames.cs` (new)
**Complexity:** Small
**Content:**
```csharp
public static class InfrastructureResourceTypeNames
{
    public const string AzureContainerAppsEnvironments = "azureContainerAppsEnvironments";
    public const string AzureContainerApps = "azureContainerApps";
    public const string AzureKubernetesServices = "azureKubernetesServices";
    public const string AzureKubernetesServiceDeployments = "azureKubernetesServiceDeployments";
}
```

#### Task 1.3: Create Infrastructure Types Constants
**File:** `src/dotnet/Common/Constants/ResourceProviders/InfrastructureTypes.cs` (new)
**Complexity:** Small
**Content:**
```csharp
public static class InfrastructureTypes
{
    public const string AzureContainerAppsEnvironment = "azure-container-apps-environment";
    public const string AzureContainerApp = "azure-container-app";
    public const string AzureKubernetesService = "azure-kubernetes-service";
    public const string AzureKubernetesServiceDeployment = "azure-kubernetes-service-deployment";
}
```

#### Task 1.4: Create Resource Provider Metadata
**File:** `src/dotnet/Common/Constants/ResourceProviders/InfrastructureResourceProviderMetadata.cs` (new)
**Complexity:** Medium
**Description:** Define allowed resource types with their HTTP methods, authorization requirements, and actions.

### Phase 2: Resource Models (Common Project)

#### Task 2.1: Create Azure Container Apps Environment Model
**File:** `src/dotnet/Common/Models/ResourceProviders/Infrastructure/AzureContainerAppsEnvironment.cs` (new)
**Complexity:** Medium
**Properties:**
- AzureResourceId, Location, ProvisioningState
- DefaultDomain, StaticIp
- WorkloadProfiles (list)

#### Task 2.2: Create Azure Container App Model
**File:** `src/dotnet/Common/Models/ResourceProviders/Infrastructure/AzureContainerApp.cs` (new)
**Complexity:** Medium
**Properties:**
- AzureResourceId, EnvironmentObjectId
- ContainerImage, Replicas (min/max)
- Ingress settings, Environment variables
- Resource limits (CPU, Memory)

#### Task 2.3: Create Azure Kubernetes Service Model
**File:** `src/dotnet/Common/Models/ResourceProviders/Infrastructure/AzureKubernetesService.cs` (new)
**Complexity:** Medium
**Properties:**
- AzureResourceId, Location, ProvisioningState
- KubernetesVersion, NodePools

#### Task 2.4: Create Azure Kubernetes Service Deployment Model
**File:** `src/dotnet/Common/Models/ResourceProviders/Infrastructure/AzureKubernetesServiceDeployment.cs` (new)
**Complexity:** Medium
**Properties:**
- ClusterObjectId, Namespace
- ContainerImage, Replicas
- Resource limits

### Phase 3: Infrastructure Project

#### Task 3.1: Create Project File
**File:** `src/dotnet/Infrastructure/Infrastructure.csproj` (new)
**Complexity:** Small
**Dependencies:** Common project

#### Task 3.2: Create Infrastructure Reference Model
**File:** `src/dotnet/Infrastructure/Models/InfrastructureReference.cs` (new)
**Complexity:** Small
**Description:** Resource reference class that maps type strings to model types.

#### Task 3.3: Create Resource Provider Service
**File:** `src/dotnet/Infrastructure/ResourceProviders/InfrastructureResourceProviderService.cs` (new)
**Complexity:** Large
**Description:** Main resource provider implementation:
- Override `GetResourceTypes()` to return metadata
- Override `GetResourcesAsync()` for reading resources
- Override `UpsertResourceAsync()` for updates
- Override `DeleteResourceAsync()` for deletions
- Override `ExecuteActionAsync()` for actions (restart, scale)

#### Task 3.4: Create Dependency Injection
**File:** `src/dotnet/Infrastructure/ResourceProviders/DependencyInjection.cs` (new)
**Complexity:** Small
**Description:** Extension method to register the resource provider.

### Phase 4: Configuration Templates (Auto-generated)

#### Task 4.1: Update DependencyInjectionKeys Template
**Note:** The `DependencyInjectionKeys.cs` is auto-generated. Need to add configuration for Infrastructure resource provider.

#### Task 4.2: Update DependencyInjection Template
**Note:** The storage configuration is auto-generated. Need to add Infrastructure storage configuration.

### Phase 5: Azure Integration (Future)

**Note:** The following tasks are for future implementation when we add actual Azure resource management:

#### Task 5.1: Azure Container Apps SDK Integration
- Add Azure.ResourceManager.AppContainers NuGet package
- Implement methods to read/update container apps

#### Task 5.2: Azure Kubernetes Service SDK Integration
- Add Azure.ResourceManager.ContainerService NuGet package
- Implement methods to read AKS clusters
- Use Kubernetes client for deployment management

## Considerations

### Assumptions
1. The infrastructure resources are already deployed and we will read their configurations
2. For the initial implementation, we will focus on reading existing resources
3. Azure SDK credentials will be managed through existing Azure Identity patterns

### Constraints
1. Must follow established resource provider patterns exactly
2. Cannot modify auto-generated template files directly
3. Limited to the infrastructure types specified in requirements

### Risks
1. **Azure SDK compatibility**: Need to ensure Azure SDK versions are compatible
   - Mitigation: Use the same SDK versions as existing Azure integrations

2. **Authorization complexity**: Infrastructure resources may need different authorization patterns
   - Mitigation: Use existing authorization patterns and extend as needed

3. **Cross-resource references**: Container apps reference environments, deployments reference clusters
   - Mitigation: Use subordinate resource type pattern (already established)

## Not Included (Future Versions)

1. Creating new Azure Container Apps Environments or AKS clusters
2. Deleting Azure Container Apps Environments or AKS clusters
3. Network configuration management
4. Secret management for infrastructure resources
5. Custom domain configuration
6. SSL certificate management
7. Monitoring and alerting configuration

## Proposed Code Structure

```
src/dotnet/Infrastructure/
├── Infrastructure.csproj
├── Models/
│   └── InfrastructureReference.cs
└── ResourceProviders/
    ├── DependencyInjection.cs
    └── InfrastructureResourceProviderService.cs

src/dotnet/Common/Constants/ResourceProviders/
├── InfrastructureResourceTypeNames.cs (new)
├── InfrastructureTypes.cs (new)
├── InfrastructureResourceProviderMetadata.cs (new)
└── ResourceProviderNames.cs (modified)

src/dotnet/Common/Models/ResourceProviders/Infrastructure/
├── AzureContainerAppsEnvironment.cs (new)
├── AzureContainerApp.cs (new)
├── AzureKubernetesService.cs (new)
└── AzureKubernetesServiceDeployment.cs (new)
```

---

**Status:** Awaiting Approval

Please review this plan and provide approval or feedback before implementation begins.
