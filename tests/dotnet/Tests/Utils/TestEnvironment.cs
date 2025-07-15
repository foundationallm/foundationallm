using FoundationaLLM.Tests.Exceptions;
using Microsoft.Extensions.Configuration;

namespace FoundationaLLM.Tests.Utils
{
    public sealed class TestEnvironment
	{
		/// <summary>
		/// Simple helper used to load env vars and secrets like credentials,
		/// to avoid hard coding them in the sample code.
		/// </summary>
		/// <param name="name">Secret name / Environment var name.</param>
		/// <returns>Value found in Secret Manager or Environment Variable.</returns>
		public static string Variable(string name)
		{
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets<TestEnvironment>()
				.Build();

			var value = configuration[name];
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}

			value = System.Environment.GetEnvironmentVariable(name);
			if (string.IsNullOrEmpty(value))
			{
				throw new TestingException($"Secret / Environment var not set: {name}");
			}

			return value;
		}
	}
}
