using FoundationaLLM.Core.Examples.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    public abstract class BaseTest(ITestOutputHelper output, IServiceProvider serviceProvider)
    {
		protected ITestOutputHelper Output { get; } = output;
		protected IServiceProvider ServiceProvider { get; } = serviceProvider;
		protected ILoggerFactory LoggerFactory { get; } = new XunitLogger(output);

		/// <summary>
		/// Service locator to get services from the ServiceProvider.
		/// </summary>
		/// <typeparam name="T">The type of service to retrieve.</typeparam>
		/// <returns></returns>
		protected T GetService<T>() where T : notnull
		{
			return ServiceProvider.GetRequiredService<T>();
		}

        protected async Task RunWithCleanup()
        {
            try
            {
                await RunExampleAsync();
            }
            catch (Exception)
            {
                await Cleanup();
                throw;
            }
            await Cleanup();
        }

        protected virtual async Task RunExampleAsync()
        {
            throw new NotImplementedException();
        }

        protected virtual async Task Cleanup()
        {
            throw new NotImplementedException();
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
	}
}
