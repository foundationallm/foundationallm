using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ict.AzureNative.CognitiveServices.inputs;
using Ict.AzureNative.Network;
using Ict.AzureNative.Network.Inputs;
using Ict.AzureNative.Utilities;
using Ict.Configuration;
using Ict.Configuration.Helper;
using Ict.PulumiBuildingBlocks.Ai_MachineLearning.inputs;
using Pulumi;
using Pulumi.Random;

namespace Ict.PulumiBuildingBlocks.Ai_MachineLearning
{
    /// <summary>
    /// Deploys a ready to use Generative AI environment in Azure using Azure Open AI services
    /// Optionally add a cognitive search service and/or Azure ML environment
    /// </summary>
    public class IctAzureGenAiEnvironment : ComponentResource
    {
        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("vNetId")]
        public Output<string>? VnetId { get; private set; }

        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("subnets")]
        public Output<ImmutableArray<string>>? Subnets { get; private set; }

        /// <summary>
        /// Id of Azure Open AI Instance
        /// </summary>
        [Output("openAiId")]
        public Output<string> OpenAiId { get; private set; }

        /// <summary>
        /// Name of Open AI Instance
        /// </summary>
        [Output("openAiName")]
        public Output<string>? OpenAiName { get; private set; }

        /// <summary>
        /// Id of Cognitive Search Service
        /// </summary>
        [Output("cognitiveSearchId")]
        public Output<string>? CognitiveSearchId { get; private set; }

        /// <summary>
        /// Name of Cognitive Search Service
        /// </summary>
        [Output("cognitiveSearchName")]
        public Output<string>? CognitiveSearchName { get; private set; }

        public IctAzureGenAiEnvironment(string name, IctAzureGenAiEnvironmentArgs args,
            ComponentResourceOptions? opts = null)
            : base("Ict:PulumiBuildingBlocks:Ai_MachineLearning:AzureGenAiEnvironmentArgs", name, opts)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNull(args, nameof(args));

            var defaultOptions = CustomResourceOptions.Merge(
                new CustomResourceOptions { Parent = this, DeleteBeforeReplace = true },
                opts.ToCustomResourceOptions()).ToComponentResourceOptions();

            var subscriptionId = Ict.AzureNative.Utilities.IctUtilities.GetCurrentSubscriptionId();

            var corpPeConfig =
                IctConfigAzure.CorporatePrivateEndpointSettings[Enum.GetName(args.CorporatePrivateEndpointNetwork)!];
            var corpSubProvider = new Pulumi.AzureNative.Provider("corpSubProvider",
                new Pulumi.AzureNative.ProviderArgs()
                {
                    SubscriptionId = corpPeConfig.SubscriptionId,
                });

            // Create Private Endpoint List and add details for corporate endpoint
            // Add any additional vNet PE's later
            var privateEndpoints = new List<IctPrivateEndpointComponentArgs>()
            {
                //Corporate Private Endpoint
                new IctPrivateEndpointComponentArgs()
                {
                    Location = corpPeConfig.Location,
                    ResourceGroupName = corpPeConfig.ResourceGroupName,
                    VirtualNetworkId = corpPeConfig.VNetId,
                    SubnetName = corpPeConfig.SubnetName,
                    Provider = corpSubProvider,
                    UseAzureDns = false
                }
            };

            Input<string> vnetId ="";
            Input<string> subnetName="";

            Ict.PulumiBuildingBlocks.Ai_MachineLearning.IctAzureMachineLearningEnvironment? mlEnvironment;

            if (args.DeployAzureMl)
            {
                mlEnvironment = new Ict.PulumiBuildingBlocks.Ai_MachineLearning.IctAzureMachineLearningEnvironment(
                    $"{name}-ml", new IctAzureMachineLearningEnvironmentArgs()
                    {
                        ResourceGroupName = args.ResourceGroupName,
                        Location = args.Location,
                        NamePrefix = args.NamePrefix,
                        AzureConfig = args.AzureConfig,
                    });
                vnetId = mlEnvironment.VnetId;
                subnetName = mlEnvironment.Subnets.Apply(x => x.First().Split('/')[10]);
                VnetId = vnetId;
                Subnets = mlEnvironment.Subnets;

            }

            if (args.DeployApplicationVnet || args.DeployAzureMl)
            {
                //If we're not deploying Azure ML then we will create the vNet, but if we need to deploy ML then we will use the vNet it creates
                if (!args.DeployAzureMl)
                {
                    var nsg = new IctNetworkSecurityGroup($"{args.NamePrefix}-core-nsg",
                        new IctNetworkSecurityGroupArgs()
                        {
                            NetworkSecurityGroupName =
                                Output.Format(
                                    $"{args.AzureConfig.NamingConventions.NetworkSecurityGroupName}-{args.NamePrefix}-core"),
                            Location = args.Location,
                            ResourceGroupName = args.ResourceGroupName,
                            Tags = args.AzureConfig.Tags,
                            RulesToDisable = ImmutableList.Create<IctRulesToDisable>(
                                IctRulesToDisable.AllowCorporateEndpointsRdpInbound,
                                IctRulesToDisable.AllowPuppetAgentsOutbound)
                        }, defaultOptions);

                    var vnet = new Ict.AzureNative.Network.IctVirtualNetwork($"{args.NamePrefix}-vnet",
                        new IctVirtualNetworkArgs()
                        {
                            Name = Output.Format(
                                $"{args.AzureConfig.NamingConventions.VirtualNetwork}-{args.NamePrefix}"),
                            Location = args.Location,
                            ResourceGroupName = args.ResourceGroupName,
                            Tags = args.AzureConfig.Tags,
                            AddressSpaces = new InputList<string>()
                            {
                                "10.0.0.0/20"
                            },
                            Subnets = new InputList<IctSubnetArgs>()
                            {
                                new IctSubnetArgs()
                                {
                                    Name = "core",
                                    AddressPrefix = "10.0.0.0/22",
                                    NetworkSecurityGroupId = nsg.NsgId
                                }
                            }
                        }, defaultOptions);
                    vnetId = vnet.VnetId;
                    subnetName = vnet.SubnetIds.Apply(subnets => subnets.First().Split("/")[10]);
                    VnetId = vnetId;
                    Subnets = vnet.SubnetIds;
                }
                
                privateEndpoints.Add(new IctPrivateEndpointComponentArgs()
                {
                    Location = args.Location,
                    ResourceGroupName = args.ResourceGroupName,
                    VirtualNetworkId = vnetId,
                    SubnetName = subnetName,
                    UseAzureDns = true
                });
            }


            var openAiService = new Ict.AzureNative.CognitiveServices.IctAzureOpenAi($"{args.NamePrefix}-openAiService",
                new IctAzureOpenAiArgs()
                {
                    Name = Output.Format($"{args.AzureConfig.NamingConventions.OpenAi}-{args.NamePrefix}"),
                    Location = args.Location,
                    ResourceGroupName = args.ResourceGroupName,
                    Tags = args.AzureConfig.Tags,
                    CustomSubDomain = args.NamePrefix,
                    Identity = new Pulumi.AzureNative.CognitiveServices.Inputs.IdentityArgs()
                    {
                        Type = Pulumi.AzureNative.CognitiveServices.ResourceIdentityType.SystemAssigned
                    },
                    PrivateEndpoints = privateEndpoints
              
                }, defaultOptions);

            OpenAiId = openAiService.Id;
            OpenAiName = openAiService.Name;

            Input<string> searchServiceId = "";

            if (args.DeployAzureCognitiveSearch)
            {
                var search = new Ict.AzureNative.CognitiveServices.IctCognitiveSearch("search",
                    new IctCognitiveSearchArgs()
                    {
                        ResourceGroupName = args.ResourceGroupName,
                        Location = args.Location,
                        Name = Output.Format($"{args.AzureConfig.NamingConventions.CognitiveSearch}-{args.NamePrefix}"),
                        Tags = args.AzureConfig.Tags,
                        PrivateEndpoints = privateEndpoints,
                        Identity = new Pulumi.AzureNative.Search.V20210401Preview.Inputs.IdentityArgs()
                        {
                            Type = Pulumi.AzureNative.Search.V20210401Preview.IdentityType.SystemAssigned
                        },
                        ServiceEndpointIpRanges = Ict.Configuration.IctConfigGlobal.AllWtwOutboundIpRanges.ToList()
                    }, defaultOptions);
                searchServiceId = search.Id;

                CognitiveSearchId = search.Id;
                CognitiveSearchName = search.Name;
            }


            foreach (var contributor in args.AzureOpenAiContributors)
            {
                _ = new Pulumi.AzureNative.Authorization.RoleAssignment(
                    name: $"{name}-aiContributor-{contributor}",
                    new Pulumi.AzureNative.Authorization.RoleAssignmentArgs
                    {
                        RoleAssignmentName = new RandomUuid(name: $"{name}-aiContributor-{contributor}").Result,
                        Scope = openAiService.Id,
                        PrincipalId = contributor,
                        RoleDefinitionId = Output.Format(
                            $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/a001fd3d-188f-4b5d-821b-7da978bf7442"),
                        PrincipalType = Pulumi.AzureNative.Authorization.PrincipalType.Group
                    }, defaultOptions.ToCustomResourceOptions());
                if (args.DeployAzureCognitiveSearch)
                {
                    _ = new Pulumi.AzureNative.Authorization.RoleAssignment(
                        name: $"{name}-searchContributor-{contributor}",
                        new Pulumi.AzureNative.Authorization.RoleAssignmentArgs
                        {
                            RoleAssignmentName = new RandomUuid(name: $"{name}-searchContributor-{contributor}").Result,
                            Scope = searchServiceId,
                            PrincipalId = contributor,
                            RoleDefinitionId = Output.Format(
                                $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/7ca78c08-252a-4471-8644-bb5ff32d4ba0"),
                            PrincipalType = Pulumi.AzureNative.Authorization.PrincipalType.Group
                        }, defaultOptions.ToCustomResourceOptions());
                }
            }

            foreach (var user in args.AzureOpenAiUsers)
            {
                _ = new Pulumi.AzureNative.Authorization.RoleAssignment(
                    name: $"{name}-aiUser-{user}",
                    new Pulumi.AzureNative.Authorization.RoleAssignmentArgs
                    {
                        RoleAssignmentName = new RandomUuid(name: $"{name}-aiUser-{user}").Result,
                        Scope = openAiService.Id,
                        PrincipalId = user,
                        RoleDefinitionId = Output.Format(
                            $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/5e0bd9bd-7b93-4f28-af87-19fc36ad61bd"),
                        PrincipalType = Pulumi.AzureNative.Authorization.PrincipalType.Group
                    }, defaultOptions.ToCustomResourceOptions());

                if (args.DeployAzureCognitiveSearch)
                {
                    _ = new Pulumi.AzureNative.Authorization.RoleAssignment(
                        name: $"{name}-searchUser-{user}",
                        new Pulumi.AzureNative.Authorization.RoleAssignmentArgs
                        {
                            RoleAssignmentName = new RandomUuid(name: $"{name}-searchUser-{user}").Result,
                            Scope = searchServiceId,
                            PrincipalId = user,
                            RoleDefinitionId = Output.Format(
                                $"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/1407120a-92aa-4202-b7e9-c0e197c71c8f"),
                            PrincipalType = Pulumi.AzureNative.Authorization.PrincipalType.Group
                        }, defaultOptions.ToCustomResourceOptions());
                }
            }
        }
    };
}