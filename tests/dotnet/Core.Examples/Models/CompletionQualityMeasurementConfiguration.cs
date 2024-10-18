namespace FoundationaLLM.Core.Examples.Models
{
    /// <summary>
    /// Agent configurations used for agent completion samples.
    /// </summary>
    public class CompletionQualityMeasurementConfiguration
	{
		/// <summary>
		/// The agent prompts to use for completions.
		/// </summary>
		public AgentPrompt[]? AgentPrompts { get; set; }
	}

	/// <summary>
	/// Defines the configuration for an agent prompt.
	/// </summary>
	public class AgentPrompt
	{
		/// <summary>
		/// The name of the agent sent to the Core API completions endpoint.
		/// </summary>
		public string? AgentName { get; set; }
		/// <summary>
		/// Indicates whether to create a new agent for the test run. If true, the agent will be created and deleted.
		/// If set to true, make sure you add the agent to the <see cref="Catalogs.AgentCatalog"/>.
		/// </summary>
        public bool CreateAgent { get; set; } = false;
		/// <summary>
		/// Controls the configuration of the chat session.
		/// </summary>
		public SessionConfiguration? SessionConfiguration { get; set; }
		/// <summary>
		/// The user prompt sent to the Core API completions endpoint.
		/// </summary>
		public string? UserPrompt { get; set; }
		/// <summary>
		/// Used for quality measurements. The expected completion for the user prompt.
		/// </summary>
		public string? ExpectedCompletion { get; set; }
	}

	/// <summary>
	/// Defines the configuration for a chat session.
	/// </summary>
	public class SessionConfiguration
	{
		/// <summary>
		/// If true, the chat session will not be stored in the database and the session ID will be ignored.
		/// </summary>
		public bool? Sessionless { get; set; }
		/// <summary>
		/// Create a new chat session rather than using an existing one.
		/// </summary>
		public bool CreateNewSession { get; set; }
		/// <summary>
		/// If you are not creating a new chat session, enter the existing session ID here.
		/// </summary>
		public string? SessionId { get; set; }
	}

}
