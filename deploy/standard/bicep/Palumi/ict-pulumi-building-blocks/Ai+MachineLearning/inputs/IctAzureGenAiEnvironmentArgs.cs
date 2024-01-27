using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ict.AzureNative.Network.Inputs;
using Ict.PulumiBuildingBlocks.Utilities;
using Pulumi;

namespace Ict.PulumiBuildingBlocks.Ai_MachineLearning.inputs
{
    public class IctAzureGenAiEnvironmentArgs : ResourceArgs
    {

        public Ict.Configuration.IctConfigAzure AzureConfig { get; set; } = null!;
        /// <summary>
        /// Prefix to be used when naming resources
        /// Required Field.
        /// </summary>
        [Input("namePrefix", required: true)]
        public string NamePrefix { get; set; } = null!;

        /// <summary>
        /// Resource Group to deploy resources to
        /// Required Field.
        /// </summary>
        [Input("resourceGroupName", required: true)]
        public Input<string> ResourceGroupName { get; set; } = null!;

        /// <summary>
        /// Location to deploy resources to
        /// Required Field.
        /// </summary>
        [Input("location", required: true)]
        public Input<string> Location { get; set; } = null!;

        /// <summary>
        /// Whether to deploy Azure ML components into this environment
        /// </summary>
        public bool DeployAzureMl { get; set; } = false;

        /// <summary>
        /// Whether to deploy Azure Cognitive Search components into this environment
        /// </summary>
        public bool DeployAzureCognitiveSearch { get; set; } = false;

        /// <summary>
        /// Selects the network to use for the corporate private endpoint
        /// </summary>
        public IctCorpPrivateEndpointEnum CorporatePrivateEndpointNetwork = IctCorpPrivateEndpointEnum.TestLabCore;

        /// <summary>
        /// Whether to deploy an Azure vNetwith Private Endpoints in addition to connecting to the corporate network
        /// Useful for where there is a need for compute resources to connect to Azure resources over Private Endpoints
        /// Will always be true if <see cref="DeployAzureMl"/> is true
        /// </summary>
        public bool DeployApplicationVnet { get; set; } = false;

        /// <summary>
        /// List of Azure AD Group object IDs to be added as contributors to the Azure Open AI environment
        /// </summary>
        public List<string> AzureOpenAiContributors { get; set; } = new List<string>();

        /// <summary>
        /// List of Azure AD Group object IDs to be added as users to the Azure Open AI environment
        /// </summary>
        public List<string> AzureOpenAiUsers { get; set; } = new List<string>();

    }
}
