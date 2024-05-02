﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Core.Examples.Exceptions;

namespace FoundationaLLM.Core.Examples.Utils
{
	internal sealed class Environment
	{
		/// <summary>
		/// Simple helper used to load env vars and secrets like credentials,
		/// to avoid hard coding them in the sample code.
		/// </summary>
		/// <param name="name">Secret name / Environment var name.</param>
		/// <returns>Value found in Secret Manager or Environment Variable.</returns>
		internal static string Variable(string name)
		{
			var configuration = new ConfigurationBuilder()
				.AddUserSecrets<Environment>()
				.Build();

			var value = configuration[name];
			if (!string.IsNullOrEmpty(value))
			{
				return value;
			}

			value = System.Environment.GetEnvironmentVariable(name);
			if (string.IsNullOrEmpty(value))
			{
				throw new FoundationaLLMException($"Secret / Environment var not set: {name}");
			}

			return value;
		}
	}
}
