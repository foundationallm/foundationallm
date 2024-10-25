using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Services.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add pipeline execution and related services the the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddPipelineExecution(this IHostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IPipelineExecutionService, PipelineExecutionService>();

            builder.Services.AddHostedService<PipelineWorker>();
        }
    }
}
