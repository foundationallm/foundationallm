using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Utils
{
    /// <summary>
    /// Logger provider for xUnit tests.
    /// </summary>
    public class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public XUnitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new XUnitLogger(_testOutputHelper, categoryName);
        }

        public void Dispose()
        {
            // No resources to dispose
        }

        private class XUnitLogger : ILogger
        {
            private readonly ITestOutputHelper _testOutputHelper;
            private readonly string _categoryName;

            public XUnitLogger(ITestOutputHelper testOutputHelper, string categoryName)
            {
                _testOutputHelper = testOutputHelper;
                _categoryName = categoryName;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (formatter == null)
                {
                    throw new ArgumentNullException(nameof(formatter));
                }

                var message = formatter(state, exception);
                if (!string.IsNullOrEmpty(message))
                {
                    _testOutputHelper.WriteLine($"{logLevel}: {_categoryName} - {message}");
                }

                if (exception != null)
                {
                    _testOutputHelper.WriteLine(exception.ToString());
                }
            }
        }
    }
}
