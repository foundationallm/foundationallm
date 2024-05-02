using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Core.Examples.Exceptions
{
    /// <summary>
    /// Basic exception for the FoundationaLLM project.
    /// </summary>
    public class FoundationaLLMException : Exception
    {
        public FoundationaLLMException() : base()
        {
        }

        public FoundationaLLMException(string message) : base(message)
        {
        }

        public FoundationaLLMException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
