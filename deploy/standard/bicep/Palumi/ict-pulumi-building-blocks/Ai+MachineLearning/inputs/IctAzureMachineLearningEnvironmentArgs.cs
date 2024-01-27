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
    public class IctAzureMachineLearningEnvironmentArgs : ResourceArgs
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
        /// Selects the network to use for the corporate private endpoint
        /// </summary>
        public IctCorpPrivateEndpointEnum CorporatePrivateEndpointNetwork = IctCorpPrivateEndpointEnum.TestLabCore;


    }
}
