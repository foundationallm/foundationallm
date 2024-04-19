using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Events
{
    /// <summary>
    /// Background worker used to execute the event service.
    /// </summary>
    /// <param name="eventService">The <see cref="IEventService"/> to run.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class EventsWorker(
        IEventService eventService,
        ILogger<EventsWorker> logger) : BackgroundService
    {
        private readonly IEventService _eventService = eventService;
        private readonly ILogger<EventsWorker> _logger = logger;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The events worker is starting up the event service.");

            try
            {
                await _eventService.StartAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The event service was not able to start. In Core API, this is not considered to be a critical error.");
            }

            _logger.LogInformation("The events worker is preparing to execute the event service.");
            await _eventService.ExecuteAsync(stoppingToken);
        }

        /// <inheritdoc/>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The events worker is stopping the event service.");
            await _eventService.StopAsync(cancellationToken);
        }
    }
}
