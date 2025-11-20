namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error related to content safety.
    /// </summary>
    public class ContentSafetyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSafetyException"/> class with a default message.
        /// </summary>
        public ContentSafetyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSafetyException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public ContentSafetyException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSafetyException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ContentSafetyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
