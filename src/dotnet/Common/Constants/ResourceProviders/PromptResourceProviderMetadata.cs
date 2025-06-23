﻿using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;

namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides metadata for the FoundationaLLM.Prompt resource provider.
    /// </summary>
    public static class PromptResourceProviderMetadata
    {
        /// <summary>
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        public static Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => new()
        {
            //{
            //    GlobalResourceTypeNames.ManagementActions,
            //    new ResourceTypeDescriptor(
            //            GlobalResourceTypeNames.ManagementActions,
            //            typeof(ResourceBase))
            //    {
            //        AllowedTypes = [],
            //        Actions = [
            //            new ResourceTypeAction(ResourceProviderActions.Trigger, false, true, [
            //                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Resource_Providers_Administrator}!", [], [typeof(ResourceProviderManagementAction)], [typeof(ResourceProviderActionResult)]),
            //            ])
            //        ]
            //    }
            //},
            {
                PromptResourceTypeNames.Prompts,
                new ResourceTypeDescriptor(
                        PromptResourceTypeNames.Prompts,
                        typeof(PromptBase))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, AuthorizableOperations.Read, [], [], [typeof(ResourceProviderGetResult<PromptBase>)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Write}|{RoleDefinitionNames.Agents_Contributor}", [], [typeof(PromptBase)], [typeof(ResourceProviderUpsertResult)]),
                        new ResourceTypeAllowedTypes(HttpMethod.Delete.Method, AuthorizableOperations.Delete, [], [], []),
                    ],
                    Actions = [
                            new ResourceTypeAction(ResourceProviderActions.CheckName, false, true, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, $"{AuthorizableOperations.Read}|{RoleDefinitionNames.Prompts_Contributor}", [], [typeof(ResourceName)], [typeof(ResourceNameCheckResult)])
                            ]),
                            new ResourceTypeAction(ResourceProviderActions.Purge, true, false, [
                                new ResourceTypeAllowedTypes(HttpMethod.Post.Method, AuthorizableOperations.Delete, [], [], [typeof(ResourceProviderActionResult)])
                            ])
                        ]
                }
            }
        };
    }
}
