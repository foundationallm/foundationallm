using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM.Core.Examples.Setup
{
    /// <summary>
    /// Provides a fullyy configured services container used to simulate an individual FoundationaLLM API instance.
    /// </summary>
    public class ServiceContainer
    {
        /// <summary>
        /// The dependency injection container.
        /// </summary>
        public required ServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// The singleton instance of the <see cref="EventsWorker"/> background service
        /// that manages the lifecycle of the Azure Event Grid event processing infrastructure.
        /// </summary>
        public IHostedService? EventsWorker { get; set; }

        /// <summary>
        /// The cancellation token source used to stop the <see cref="EventsWorker"/> background service.
        /// </summary>
        public CancellationTokenSource EventsWorkerCancellationTokenSource = new();
    }
}
