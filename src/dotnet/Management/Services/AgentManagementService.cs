using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Vectorization.Models.Resources;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Agent.ResourceProviders;
using FoundationaLLM.Vectorization.ResourceProviders;

namespace FoundationaLLM.Management.Services
{
    public class AgentManagementService(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)]
        IResourceProviderService vectorizationResourceProviderService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Agent_ResourceProviderService)]
        IResourceProviderService agentResourceProviderService)
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService =
            vectorizationResourceProviderService;
        private readonly IResourceProviderService _agentResourceProviderService =
            agentResourceProviderService;

        private List<ContentSourceProfile>? GetVectorContentSourceProfiles()
        {
            var contentSourceProfiles = _vectorizationResourceProviderService.GetResources<ContentSourceProfile>(
                                           $"/{VectorizationResourceTypeNames.ContentSourceProfiles}");

            return contentSourceProfiles as List<ContentSourceProfile>;
        }

        private List<IndexingProfile>? GetVectorIndexingProfiles()
        {
            var indexingProfiles = _vectorizationResourceProviderService.GetResources<IndexingProfile>(
                                                      $"/{VectorizationResourceTypeNames.IndexingProfiles}");

            return indexingProfiles as List<IndexingProfile>;
        }

        private List<ContentSourceProfile>? GetContentSourceProfiles()
        {
            var contentSourceProfiles = _vectorizationResourceProviderService.GetResources<ContentSourceProfile>(
                                                          $"/{VectorizationResourceTypeNames.ContentSourceProfiles}");

            return contentSourceProfiles as List<ContentSourceProfile>;
        }

        private List<IndexingProfile>? GetIndexingProfiles()
        {
            var indexingProfiles = _vectorizationResourceProviderService.GetResources<IndexingProfile>(
                                                                    $"/{VectorizationResourceTypeNames.IndexingProfiles}");

            return indexingProfiles as List<IndexingProfile>;
        }

        private List<AgentReference>? GetAgentReferences()
        {
            var agentReferences = _agentResourceProviderService.GetResources<AgentReference>(
                                                                                   $"/{AgentResourceTypeNames.AgentReferences}");

            return agentReferences as List<AgentReference>;
        }
    }
}
