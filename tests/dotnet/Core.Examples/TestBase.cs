using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    public abstract class TestBase
    {
		protected ITestOutputHelper Output { get; }
		protected List<IServiceProvider> ServiceProviders { get; }
        protected IServiceProvider ServiceProvider { get; }

		public TestBase(
			ITestOutputHelper output,
            TestFixture fixture)
		{
			Output = output;
			ConfigureServiceProviders(fixture, output);

			ServiceProviders = fixture.ServiceProviders;
			ServiceProvider = fixture.ServiceProviders.First();
        }

		protected virtual void ConfigureServiceProviders(TestFixture fixture, ITestOutputHelper output) =>
            fixture.ConfigureServiceProviders(
                1,
                output,
                (hostBuilder, testOutputHelper, virtualHostsCount) =>
                {
                    var _serviceCollection = new ServiceCollection();

                    TestServicesInitializer.InitializeServices(
                        _serviceCollection,
                        hostBuilder.Configuration,
                        testOutputHelper);

                    return [_serviceCollection.BuildServiceProvider()];
                });

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
	}
}
