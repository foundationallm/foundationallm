using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ict.AzureNative.CognitiveServices.inputs;
using Ict.AzureNative.ContainerRegistry.Inputs;
using Ict.AzureNative.Network;
using Ict.AzureNative.Network.Inputs;
using Ict.AzureNative.Security.Inputs;
using Ict.AzureNative.Storage.Inputs;
using Ict.AzureNative.Utilities;
using Ict.Configuration;
using Ict.PulumiBuildingBlocks.Ai_MachineLearning.inputs;
using Ict.PulumiBuildingBlocks.Extensions;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.MachineLearningServices.Inputs;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Kind = Pulumi.AzureNative.Storage.Kind;

namespace Ict.PulumiBuildingBlocks.Ai_MachineLearning
{
    /// <summary>
    /// Creates a ready to use Azure Machine Learning Environment including required resources for ACR, Storage, Key Vault and App Insights as well as vNet with private endpoints for running any required compute
    /// </summary>
    public class IctAzureMachineLearningEnvironment : ComponentResource
    {
        /// <summary>
        /// ID of the Azure Ml Workspace created
        /// </summary>
        [Output("mlWorkspaceId")]
        public Output<string> MlWorkspaceId { get; private set; }

        /// <summary>
        /// Name of the Azure Ml Workspace created
        /// </summary>
        [Output("mlWorkspaceName")]
        public Output<string> MlWorkspaceName { get; private set; }

        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("vNetId")]
        public Output<string> VnetId { get; private set; }

        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("subnets")]
        public Output<ImmutableArray<string>> Subnets { get; private set; }

        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("keyVaultId")]
        public Output<string> KeyVaultId { get; private set; }


        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("keyVaultName")]
        public Output<string> KeyVaultName { get; private set; }


        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("mlStorageId")]
        public Output<string> MlStorageId { get; private set; }


        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("mlStorageName")]
        public Output<string> MlStorageName{ get; private set; }


        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("acrId")]
        public Output<string> AcrId { get; private set; }


        /// <summary>
        /// Id of vNet Created
        /// </summary>
        [Output("acrName")]
        public Output<string> AcrName { get; private set; }


        public IctAzureMachineLearningEnvironment(string name, IctAzureMachineLearningEnvironmentArgs args,
            ComponentResourceOptions? opts = null)
            : base("Ict:PulumiBuildingBlocks:Ai_MachineLearning:AzureGenAiEnvironmentArgs", name, opts)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNull(args, nameof(args));

            var defaultOptions = CustomResourceOptions.Merge(
                new CustomResourceOptions { Parent = this, DeleteBeforeReplace = true },
                opts.ToCustomResourceOptions()).ToComponentResourceOptions();

            var corpPeConfig =
                IctConfigAzure.CorporatePrivateEndpointSettings[Enum.GetName(args.CorporatePrivateEndpointNetwork)!];
            var corpSubProvider = new Pulumi.AzureNative.Provider($"{name}corpSubProvider-ml",
                new Pulumi.AzureNative.ProviderArgs()
                {
                    SubscriptionId = corpPeConfig.SubscriptionId,
                });

            var privateEndpoints = new List<IctPrivateEndpointComponentArgs>()
            {
                //Corporate Private Endpoint
                new IctPrivateEndpointComponentArgs()
                {
                    Location = args.Location,
                    ResourceGroupName = corpPeConfig.ResourceGroupName,
                    VirtualNetworkId = corpPeConfig.VNetId,
                    SubnetName = corpPeConfig.SubnetName,
                    Provider = corpSubProvider,
                    UseAzureDns = false
                }
            };


            var nsg = new IctNetworkSecurityGroup($"{args.NamePrefix}-core-nsg", new IctNetworkSecurityGroupArgs()
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
                    Name = Output.Format($"{args.AzureConfig.NamingConventions.VirtualNetwork}-{args.NamePrefix}"),
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

            VnetId = vnet.VnetId;
            Subnets = vnet.SubnetIds;

            privateEndpoints.Add(new IctPrivateEndpointComponentArgs()
            {
                Location = args.Location,
                ResourceGroupName = args.ResourceGroupName,
                VirtualNetworkId = vnet.VnetId,
                SubnetName = "core",
                UseAzureDns = true
            });


            var keyVault = new Ict.AzureNative.Security.IctKeyVault($"{name}-ml-keyVault", new IctKeyVaultArgs()
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = args.Location,
                Tags = args.AzureConfig.Tags,
                Name = Output.Format($"{args.AzureConfig.NamingConventions.KeyVault}-{args.NamePrefix}"),
                EnableRbacAuthorization = true,
                EnabledForTrustedMicrosoftServices = true,
                UseRandomSuffix = true,
            }, defaultOptions);

            KeyVaultName = keyVault.KeyVaultName;
            KeyVaultId = keyVault.KeyVaultId;

            var mlStorage = new Ict.AzureNative.Storage.IctStorageAccount($"{name}-ml-storage",
                new IctStorageAccountArgs()
                {
                    ResourceGroupName = args.ResourceGroupName,
                    Location = args.Location,
                    Tags = args.AzureConfig.Tags,
                    Name = Output.Format($"{args.AzureConfig.NamingConventions.Storage}{args.NamePrefix}ml"),
                    Kind = Kind.StorageV2,
                    SkuName = SkuName.Standard_LRS,
                    AccessTier = AccessTier.Hot
                });

            MlStorageId = mlStorage.StorageAccountId;
            MlStorageName = mlStorage.StorageAccountName;

            var acr = new Ict.AzureNative.ContainerRegistry.IctRegistry($"{name}-ml-acr", new IctRegistryArgs()
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = args.Location,
                Tags = args.AzureConfig.Tags,
                Name = Output.Format($"{args.AzureConfig.NamingConventions.Acr}{args.NamePrefix}"),
                IpRules = new InputList<string>()
            });

            AcrId = acr.Id;
            AcrName = acr.Name;

            var appInsights = new Pulumi.AzureNative.Insights.Component($"{name}-app-insights", new ComponentArgs()
            {
                ResourceGroupName = args.ResourceGroupName,
                Location = args.Location,
                Tags = args.AzureConfig.Tags,
                ApplicationType = ApplicationType.Web,
                Kind = "web",
                IngestionMode = IngestionMode.ApplicationInsights,
                ResourceName = Output.Format($"{args.AzureConfig.NamingConventions.AppInsights}-{args.NamePrefix}"),
            });

            if (privateEndpoints.Count > 0)
            {
                foreach (var (privateEndpoint, index) in privateEndpoints.WithIndex())
                {
                    if (privateEndpoint.Provider is not null)
                    {
                        defaultOptions!.Provider = privateEndpoint.Provider;
                    }
                    else
                    {
                        defaultOptions!.Provider = null;
                    }



                    CreatePrivateEndpoint($"{name}-keyvault-pe{index}",
                        Output.Format($"{keyVault.KeyVaultName}-pe{index}"),
                        privateEndpoint, keyVault.KeyVaultId, new InputList<string>() { "vault" },
                        privateEndpoint.UseAzureDns
                            ? new List<IIctPrivateEndpointDnsZoneArgs>()
                            {
                                new IctNewPrivateEndpointDnsZoneArgs()
                                {
                                    DnsZoneName = "privatelink.vaultcore.azure.net"
                                },
                            }
                            : new List<IIctPrivateEndpointDnsZoneArgs>(),
                        defaultOptions);


                    CreatePrivateEndpoint($"{name}-mlstorage-pe{index}",
                        Output.Format($"{mlStorage.StorageAccountName}-pe{index}"),
                        privateEndpoint, mlStorage.StorageAccountId, new InputList<string>() { "blob" },
                        privateEndpoint.UseAzureDns
                            ? new List<IIctPrivateEndpointDnsZoneArgs>()
                            {
                                new IctNewPrivateEndpointDnsZoneArgs()
                                {
                                    DnsZoneName = "privatelink.blob.core.windows.net"
                                },
                            }
                            : new List<IIctPrivateEndpointDnsZoneArgs>(),
                        defaultOptions);


                    CreatePrivateEndpoint($"{name}-acr-pe{index}",
                        Output.Format($"{acr.Name}-pe{index}"),
                        privateEndpoint, acr.Id, new InputList<string>() { "registry" },
                        privateEndpoint.UseAzureDns
                            ? new List<IIctPrivateEndpointDnsZoneArgs>()
                            {
                                new IctNewPrivateEndpointDnsZoneArgs()
                                {
                                    DnsZoneName = "privatelink.azurecr.io"
                                },
                            }
                            : new List<IIctPrivateEndpointDnsZoneArgs>(),
                        defaultOptions);
                }
            }

            var mlEnvironment = new Ict.AzureNative.CognitiveServices.IctMachineLearningWorkspace($"{name}-ml",
                new IctMachineLearningWorkspaceArgs()
                {
                    ResourceGroupName = args.ResourceGroupName,
                    Location = args.Location,
                    Tags = args.AzureConfig.Tags,
                    Name = Output.Format($"{args.AzureConfig.NamingConventions.MachineLearning}{args.NamePrefix}"),
                    StorageAccountId = mlStorage.StorageAccountId,
                    KeyVaultId = keyVault.KeyVaultId,
                    ContainerRegistryId = acr.Id,
                    AppInsightsId = appInsights.Id,
                    PrivateEndpoints = privateEndpoints,
                    Identity = new ManagedServiceIdentityArgs()
                    {
                        Type = Pulumi.AzureNative.MachineLearningServices.ManagedServiceIdentityType.SystemAssigned
                    }


                });
            MlWorkspaceId = mlEnvironment.Id;
            MlWorkspaceName = mlEnvironment.Name;
  
        }

        private static IctPrivateEndpoint CreatePrivateEndpoint(string pulumiName, Input<string> name,
            IctPrivateEndpointComponentArgs args, Input<string> resourceId, InputList<string> subResourceNames,
            List<IIctPrivateEndpointDnsZoneArgs> zones, ComponentResourceOptions opts)
        {
            return new IctPrivateEndpoint(pulumiName, new IctPrivateEndpointArgs()
            {
                Location = args.Location,
                ResourceGroupName = args.ResourceGroupName,
                SubnetName = args.SubnetName,
                VirtualNetworkId = args.VirtualNetworkId,
                TargetResourceId = resourceId,
                Name = name,
                SubResourceNames = subResourceNames,
                PrivateZones = zones
            }, opts);
        }
    };
}