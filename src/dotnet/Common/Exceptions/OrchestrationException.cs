﻿namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error with accessing content.
    /// </summary>
    public class OrchestrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationException"/> class with a default message.
        /// </summary>
        public OrchestrationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public OrchestrationException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public OrchestrationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
