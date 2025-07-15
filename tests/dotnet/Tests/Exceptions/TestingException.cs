namespace FoundationaLLM.Tests.Exceptions
{
    /// <summary>
    /// Basic exception for the FoundationaLLM project.
    /// </summary>
    public class TestingException : Exception
    {
        public TestingException() : base()
        {
        }

        public TestingException(string message) : base(message)
        {
        }

        public TestingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
