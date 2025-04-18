namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error originating in a plugin.
    /// </summary>
    public class PluginException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginException"/> class with a default message.
        /// </summary>
        public PluginException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public PluginException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public PluginException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
