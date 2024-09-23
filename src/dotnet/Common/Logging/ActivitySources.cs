using FoundationaLLM.Common.Constants;
using System.Diagnostics;

namespace FoundationaLLM.Common.Logging
{
    public class ActivitySources
    {
        public static readonly ActivitySource AgentHubAPIActivitySource = new(ServiceNames.AgentHubAPI);
        public static readonly ActivitySource CoreAPIActivitySource = new(ServiceNames.CoreAPI);
        public static readonly ActivitySource GatekeeperAPIActivitySource = new(ServiceNames.GatekeeperAPI);
        public static readonly ActivitySource GatewayAdapterAPIActivitySource = new(ServiceNames.GatewayAdapterAPI);
        public static readonly ActivitySource ManagementAPIActivitySource = new(ServiceNames.ManagementAPI);
        public static readonly ActivitySource OrchestrationAPIActivitySource = new(ServiceNames.OrchestrationAPI);
        public static readonly ActivitySource SemanticKernelAPIActivitySource = new(ServiceNames.SemanticKernelAPI);
        public static readonly ActivitySource StateAPIActivitySource = new(ServiceNames.StateAPI);
        public static readonly ActivitySource VectorizationAPIActivitySource = new(ServiceNames.VectorizationAPI);

        public static Activity StartActivity(string name, ActivitySource source, ActivityKind kind = System.Diagnostics.ActivityKind.Consumer, bool addBaggage = true)
        {
            var activity = source.StartActivity(name, kind);

            if (addBaggage && activity != null)
            {
                foreach (var bag in activity?.Parent?.Baggage)
                {
                    activity?.AddTag(bag.Key, bag.Value);
                }
            }

            return activity;
        }
    }
}
