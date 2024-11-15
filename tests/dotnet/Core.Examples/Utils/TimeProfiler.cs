using System.Diagnostics;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Utils
{
    public class TimeProfiler(
        ITestOutputHelper output)
    {
        private readonly ITestOutputHelper _output = output;


        public async Task RunAsync(Func<Task> action, string actionName)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await action();

            stopwatch.Stop();
            _output.WriteLine($"{actionName} took {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
