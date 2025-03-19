using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Events;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    public abstract class TestBase
    {
		protected ITestOutputHelper Output { get; }
		protected List<IServiceProvider> ServiceProviders { get; }
        protected IServiceProvider ServiceProvider { get; }

		private IHostedService? _eventsWorker;
		private CancellationTokenSource _eventsWorkerCancellationTokenSource = new();

		public TestBase(
			ITestOutputHelper output,
            TestFixture fixture)
		{
			Output = output;

			ServiceProviders = GetServiceProviders(output, fixture);
			ServiceProvider = ServiceProviders.First();
        }

		protected virtual List<IServiceProvider> GetServiceProviders(
			ITestOutputHelper output,
			TestFixture fixture)
        {
            var _serviceCollection = new ServiceCollection();

            DependencyInjectionContainerInitializer.InitializeServices(
                _serviceCollection,
                fixture.HostBuilder.Configuration,
                output);

            return [_serviceCollection.BuildServiceProvider()];
        }

        /// <summary>
        /// Service locator to get services from the ServiceProvider.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve.</typeparam>
        /// <returns></returns>
        protected T GetService<T>() where T : notnull
		{
			return ServiceProvider.GetRequiredService<T>();
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

		/// <summary>
		/// Starts the <see cref="EventsWorker"/> background service which manages the 
		/// lifetime of the <see cref="AzureEventGridEventService"/> events service.
		/// </summary>
		protected async Task StartEventsWorker()
        {
            _eventsWorker = ServiceProvider
				.GetRequiredService<IEnumerable<IHostedService>>()
				.SingleOrDefault(x => x.GetType() == typeof(EventsWorker));
            _ = _eventsWorker!.StartAsync(_eventsWorkerCancellationTokenSource.Token);

            WriteLine("Waiting for the AzureEventGridService to become active...");
			var eventService = ServiceProvider.GetRequiredService<IEventService>();
			
			await eventService.InitializationTask;
			WriteLine($"AzureEventGridService is active with initialization status = {(eventService.InitializationTask.Result ? "Success" : "Error" )}.");

			if (!eventService.InitializationTask.Result)
				throw new Exception("AzureEventGridService initialization failed.");
        }

        /// <summary>
        /// Sends a cancellation request to the <see cref="EventsWorker"/> background service 
		/// which manages the lifetime of the <see cref="AzureEventGridEventService"/> events service.
		/// Once the cancellation request is processed, the service will stop.
        /// </summary>
        protected async Task StopEventsWorker()
		{
			if (_eventsWorker == null)
				return;

            _eventsWorkerCancellationTokenSource.Cancel();
			await _eventsWorker.StopAsync(default);
        }

		protected async Task StartQuotaService()
		{
            var quotaService = ServiceProvider.GetRequiredService<IQuotaService>();

            WriteLine("Waiting for the QuotaService to become active...");
            await quotaService.InitializationTask;
            WriteLine($"QuotaService is active with initialization status = {(quotaService.InitializationTask.Result ? "Success" : "Error")}.");

            if (!quotaService.InitializationTask.Result)
                throw new Exception("QuotaService initialization failed.");
        }
    }
}
