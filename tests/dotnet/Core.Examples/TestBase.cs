using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Common.Services.Events;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    public class TestBase
    {
		protected ITestOutputHelper Output { get; }

		protected List<ServiceContainer> ServiceContainers { get; }
        
		protected ServiceContainer MainServiceContainer { get; }

		protected readonly int _serviceContainerCount;

		public TestBase(
			int serviceContainerCount,
            ITestOutputHelper output,
            TestFixture fixture)
		{
            _serviceContainerCount = serviceContainerCount;

            Output = output;
			ServiceContainers = GetServiceContainers(output, fixture);
			MainServiceContainer = ServiceContainers.First();
        }

		protected virtual List<ServiceContainer> GetServiceContainers(
			ITestOutputHelper output,
			TestFixture fixture) =>
            [.. Enumerable.Range(1, _serviceContainerCount)
				.Select(serviceContainerIndex =>
				{
                    var serviceCollection = new ServiceCollection();

                    DependencyInjectionContainerInitializer.InitializeServices(
                        serviceContainerIndex,
                        serviceCollection,
                        fixture.HostBuilder.Configuration,
                        output);

					return new ServiceContainer
					{
						ServiceProvider = serviceCollection.BuildServiceProvider()
                    };
                })];

        /// <summary>
        /// Service locator to get services from the main service container's service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns></returns>
        protected T GetService<T>() where T : notnull
		{
			return MainServiceContainer.ServiceProvider.GetRequiredService<T>();
		}

		/// <summary>
		/// This method can be substituted by Console.WriteLine when used in a Console apps.
		/// </summary>
		/// <param name="target">Target object to write</param>
		protected void WriteLine(object? target = null)
		{
			this.Output.WriteLine((string)(target ?? string.Empty));
		}

		/// <summary>
		/// Current interface ITestOutputHelper does not have a Write method. This extension
		/// method adds it to make it analogous to Console.Write when used in a Console apps.
		/// </summary>
		/// <param name="target">Target object to write</param>
		protected void Write(object? target = null)
		{
			this.Output.WriteLine((string)(target ?? string.Empty));
		}

		protected async Task StartEventsWorkers() =>
            await Task.WhenAll(
				ServiceContainers.Select(sc => StartEventsWorker(sc)));

		protected async Task StopEventsWorkers() =>
            await Task.WhenAll(
                ServiceContainers.Select(sc => StopEventsWorker(sc)));

		protected async Task WaitForQuotaServices() =>
            await Task.WhenAll(
                ServiceContainers.Select(sc => WaitForQuotaService(sc)));

        /// <summary>
        /// Starts the <see cref="EventsWorker"/> background service which manages the 
        /// lifetime of the <see cref="AzureEventGridEventService"/> events service.
        /// </summary>
		/// <param name="serviceContainer">The <see cref="ServiceContainer"/> providing the service.</param>
        protected async Task StartEventsWorker(ServiceContainer serviceContainer)
        {
			var containerProperties = serviceContainer.ServiceProvider
				.GetRequiredService<DependencyInjectionContainerSettings>();

            serviceContainer.EventsWorker = serviceContainer.ServiceProvider
				.GetRequiredService<IEnumerable<IHostedService>>()
				.SingleOrDefault(x => x.GetType() == typeof(EventsWorker));
            _ = serviceContainer.EventsWorker!.StartAsync(
				serviceContainer.EventsWorkerCancellationTokenSource.Token);

            WriteLine($"Service container {containerProperties.Id:D3}: Waiting for the AzureEventGridService in to become active...");
			
			var eventService = serviceContainer.ServiceProvider.GetRequiredService<IEventService>();
			
			await eventService.InitializationTask;
			WriteLine($"Service container {containerProperties.Id:D3}: AzureEventGridService is active with initialization status = {(eventService.InitializationTask.Result ? "Success" : "Error" )}.");

			if (!eventService.InitializationTask.Result)
				throw new Exception($"Service container {containerProperties.Id:D3}: AzureEventGridService initialization failed.");
        }

        /// <summary>
        /// Sends a cancellation request to the <see cref="EventsWorker"/> background service 
		/// which manages the lifetime of the <see cref="AzureEventGridEventService"/> events service.
		/// Once the cancellation request is processed, the service will stop.
        /// </summary>
		/// <param name="serviceContainer">The <see cref="ServiceContainer"/> providing the service.</param>
        protected static async Task StopEventsWorker(ServiceContainer serviceContainer)
		{
			if (serviceContainer.EventsWorker == null)
				return;

            serviceContainer.EventsWorkerCancellationTokenSource.Cancel();
			await serviceContainer.EventsWorker.StopAsync(default);
        }

        /// <summary>
        /// Waits for the <see cref="QuotaService"/> singleton service to complete its startup.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="ServiceContainer"/> providing the service.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected async Task WaitForQuotaService(ServiceContainer serviceContainer)
		{
            var containerProperties = serviceContainer.ServiceProvider
                .GetRequiredService<DependencyInjectionContainerSettings>();
            var quotaService = serviceContainer.ServiceProvider
				.GetRequiredService<IQuotaService>();

            WriteLine($"Service container {containerProperties.Id:D3}: Waiting for the QuotaService to become active...");
            await quotaService.InitializationTask;
            WriteLine($"Service container {containerProperties.Id:D3}: QuotaService is active with initialization status = {(quotaService.InitializationTask.Result ? "Success" : "Error")}.");

            if (!quotaService.InitializationTask.Result)
                throw new Exception($"Service container {containerProperties.Id:D3}: QuotaService initialization failed.");
        }
    }
}
