using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using System.Collections.ObjectModel;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents all role definitions used in RBAC.
    /// </summary>
    public static class RoleDefinitions
    {
        public static readonly ReadOnlyDictionary<string, RoleDefinition> All = new (
            new Dictionary<string, RoleDefinition>()
            {
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/17ca4b59-3aee-497d-b43b-95dd7d916f99",
                    new RoleDefinition
                    {
                        Name = "17ca4b59-3aee-497d-b43b-95dd7d916f99",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/17ca4b59-3aee-497d-b43b-95dd7d916f99",
                        DisplayName = "Role Based Access Control Administrator",
                        Description = "Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Authorization/roleAssignments/read",
                                    "FoundationaLLM.Authorization/roleAssignments/write",
                                    "FoundationaLLM.Authorization/roleAssignments/delete",
                                    "FoundationaLLM.Authorization/roleDefinitions/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                    new RoleDefinition
                    {
                        Name = "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                        DisplayName = "Reader",
                        Description = "View all resources without the possiblity of making any changes.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                    new RoleDefinition
                    {
                        Name = "a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                        DisplayName = "Contributor",
                        Description = "Full access to manage all resources without the possiblity of assigning roles in FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*",],
                                NotActions = [
                                    "FoundationaLLM.Authorization/*/write",
                                    "FoundationaLLM.Authorization/*/delete",],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                    new RoleDefinition
                    {
                        Name = "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                        DisplayName = "User Access Administrator",
                        Description = "Manage access to FoundationaLLM resources.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*/read",
                                    "FoundationaLLM.Authorization/*",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
                    new RoleDefinition
                    {
                        Name = "1301f8d4-3bea-4880-945f-315dbd2ddb46",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
                        DisplayName = "Owner",
                        Description = "Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
                    new RoleDefinition
                    {
                        Name = "8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/8e77fb6a-7a78-43e1-b628-d9e2285fe25a",
                        DisplayName = "Attachments Contributor",
                        Description = "Upload attachments including uploading to Azure OpenAI file store.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Attachment/attachments/read",
                                    "FoundationaLLM.Attachment/attachments/write",
                                    "FoundationaLLM.AzureOpenAI/conversationMappings/read",
                                    "FoundationaLLM.AzureOpenAI/conversationMappings/write",
                                    "FoundationaLLM.AzureOpenAI/fileMappings/read",
                                    "FoundationaLLM.AzureOpenAI/fileMappings/write",
                                    "FoundationaLLM.Configuration/apiEndpointConfigurations/read",
                                    "FoundationaLLM.AIModel/aiModels/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/d0d21b90-5317-499a-9208-3a6cb71b84f9",
                    new RoleDefinition
                    {
                        Name = "d0d21b90-5317-499a-9208-3a6cb71b84f9",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/d0d21b90-5317-499a-9208-3a6cb71b84f9",
                        DisplayName = "Conversations Contributor",
                        Description = "Create and update conversations, including Azure OpenAI Assistants threads.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Conversation/conversations/read",
                                    "FoundationaLLM.Conversation/conversations/write",
                                    "FoundationaLLM.AzureOpenAI/conversationMappings/read",
                                    "FoundationaLLM.AzureOpenAI/conversationMappings/write",
                                    "FoundationaLLM.Configuration/apiEndpointConfigurations/read",
                                    "FoundationaLLM.AIModel/aiModels/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-10-22T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-10-22T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/2da16a58-ed63-431a-b90e-9df32c2cae4a",
                    new RoleDefinition
                    {
                        Name = "2da16a58-ed63-431a-b90e-9df32c2cae4a",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/2da16a58-ed63-431a-b90e-9df32c2cae4a",
                        DisplayName = "Data Pipelines Contributor",
                        Description = "Create new data pipelines.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Configuration/apiEndpointConfigurations/read",
                                    "FoundationaLLM.AIModel/aiModels/read",
                                    "FoundationaLLM.Plugin/plugins/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/e959eecb-8edf-4442-b532-4990f9a1df2b",
                    new RoleDefinition
                    {
                        Name = "e959eecb-8edf-4442-b532-4990f9a1df2b",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/e959eecb-8edf-4442-b532-4990f9a1df2b",
                        DisplayName = "Data Pipelines Execution Manager",
                        Description = "Manage all aspects related to data pipeline runs.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.DataSource/dataSources/read",
                                    "FoundationaLLM.Configuration/apiEndpointConfigurations/read",
                                    "FoundationaLLM.AIModel/aiModels/read",
                                    "FoundationaLLM.Plugin/plugins/read",
                                    "FoundationaLLM.Vector/vectorDatabases/read",
                                    "FoundationaLLM.DataPipeline/dataPipelines/read",
                                    "FoundationaLLM.DataPipeline/dataPipelines/write",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/3f28aa77-a854-4aa7-ae11-ffda238275c9",
                    new RoleDefinition
                    {
                        Name = "3f28aa77-a854-4aa7-ae11-ffda238275c9",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/3f28aa77-a854-4aa7-ae11-ffda238275c9",
                        DisplayName = "Agents Contributor",
                        Description = "Create new agents.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/8c5ea0d3-f5a1-4be5-90a7-a12921c45542",
                    new RoleDefinition
                    {
                        Name = "8c5ea0d3-f5a1-4be5-90a7-a12921c45542",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/8c5ea0d3-f5a1-4be5-90a7-a12921c45542",
                        DisplayName = "Agent Access Tokens Contributor",
                        Description = "Create new agent access tokens.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/479e7b36-5965-4a7f-baf7-84e57be854aa",
                    new RoleDefinition
                    {
                        Name = "479e7b36-5965-4a7f-baf7-84e57be854aa",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/479e7b36-5965-4a7f-baf7-84e57be854aa",
                        DisplayName = "Prompts Contributor",
                        Description = "Create new prompts.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/8eec6664-9abf-4beb-84f7-18d9c2917c7f",
                    new RoleDefinition
                    {
                        Name = "8eec6664-9abf-4beb-84f7-18d9c2917c7f",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/8eec6664-9abf-4beb-84f7-18d9c2917c7f",
                        DisplayName = "Knowledge Sources Contributor",
                        Description = "Create new knowledge sources.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/78ee11d9-6e6a-4adc-8c16-3613e7445113",
                    new RoleDefinition
                    {
                        Name = "78ee11d9-6e6a-4adc-8c16-3613e7445113",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/78ee11d9-6e6a-4adc-8c16-3613e7445113",
                        DisplayName = "Data Sources Contributor",
                        Description = "Create new data sources.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/5f38b653-e3b7-47a8-8fde-e70ea9e4fa91",
                    new RoleDefinition
                    {
                        Name = "5f38b653-e3b7-47a8-8fde-e70ea9e4fa91",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/5f38b653-e3b7-47a8-8fde-e70ea9e4fa91",
                        DisplayName = "Knowledge Units Contributor",
                        Description = "Create new knowledge units.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2026-01-10T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/c026f070-abc2-4419-aed9-ec0676f81519",
                    new RoleDefinition
                    {
                        Name = "c026f070-abc2-4419-aed9-ec0676f81519",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/c026f070-abc2-4419-aed9-ec0676f81519",
                        DisplayName = "Vector Databases Contributor",
                        Description = "Create new vector databases.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Configuration/apiEndpointConfigurations/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/63b6cc4d-9e1c-4891-8201-cf58286ebfe6",
                    new RoleDefinition
                    {
                        Name = "63b6cc4d-9e1c-4891-8201-cf58286ebfe6",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/63b6cc4d-9e1c-4891-8201-cf58286ebfe6",
                        DisplayName = "Resource Providers Administrator",
                        Description = "Execute management actions on resource providers.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*/management/write",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2025-05-01T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
            });
    }
}
