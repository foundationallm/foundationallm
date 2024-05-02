using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Core.Examples.Exceptions
{
	/// <summary>
	/// Exception thrown when a configuration key is not found.
	/// </summary>
	public sealed class ConfigurationNotFoundException : Exception
	{
		public string? Section { get; }
		public string? Key { get; }

		public ConfigurationNotFoundException(string section, string key)
			: base($"Configuration key '{section}:{key}' not found")
		{
			this.Section = section;
			this.Key = key;
		}

		public ConfigurationNotFoundException(string section)
			: base($"Configuration section '{section}' not found")
		{
			this.Section = section;
		}

		public ConfigurationNotFoundException() : base()
		{
		}

		public ConfigurationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}
