using FoundationaLLM.CSharpCodeSession.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace FoundationaLLM.CSharpCodeSession.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController(ILogger<CodeController> logger) : ControllerBase
    {
        private readonly ILogger<CodeController> _logger = logger;

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteCode([FromBody] CodeExecutionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("Invalid code execution request.");

            try
            {
                var result = await CSharpScript.EvaluateAsync(
                    request.Code,
                    ScriptOptions.Default);

                return Ok(new CodeExecutionResponse
                {
                    Results = result,
                    Output = string.Empty
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code.");
                return StatusCode(500, "Error executing code.");
            }
        }
    }
}
