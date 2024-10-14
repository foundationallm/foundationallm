using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.State.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Dynamic;
using System.Text.Json;

namespace FoundationaLLM.State.API.Controllers
{
    /// <summary>
    /// Provides methods for managing long-running operations.
    /// </summary>
    /// <param name="stateService">Provides methods for managing state for long-running operations.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class OperationsController(IStateService stateService) : ControllerBase
    {
        /// <summary>
        /// Retrieves all long-running operations.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetLongRunningOperations()
        {
            var operations = await stateService.GetLongRunningOperations();
            return Ok(operations);
        }

        /// <summary>
        /// Retrieves a long-running operation by its identifier.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <returns></returns>
        [HttpGet("{operationId}")]
        public async Task<IActionResult> GetLongRunningOperation(string instanceId, string operationId)
        {
            var operation = await stateService.GetLongRunningOperation(operationId);
            if (operation == null)
            {
                return NotFound();
            }
            return Ok(operation);
        }

        /// <summary>
        /// Retrieves all log entries for a long-running operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <returns></returns>
        [HttpGet("{operationId}/logs")]
        public async Task<IActionResult> GetLongRunningOperationLogs(string instanceId, string operationId)
        {
            var logEntries = await stateService.GetLongRunningOperationLogEntries(operationId);
            return Ok(logEntries);
        }

        /// <summary>
        /// Inserts a long-running operation and creates a log entry.
        /// </summary>
        /// <param name="operationId">The ID of the long running operation.</param>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="body">The request body, containing the identity of the user who triggered the long-running operation.</param>
        /// <returns></returns>
        [HttpPost("{operationId}")]
        public async Task<IActionResult> CreateLongRunningOperation(string instanceId, string operationId, [FromBody] JsonElement body)
        {
            var requestBody = JsonSerializer.Deserialize<Dictionary<string, Object>>(body);
            if (requestBody == null)
            {
                return BadRequest("A request body containing the user principal name (UPN) of the user submitting the operation is required.");
            }
            var upn = requestBody["upn"].ToString();
            if (upn == null)
            {
                return BadRequest("The user principal name (UPN) of the user submitting the operation was not found in the request body. It is required.");
            }

            var createdLongRunningOperation = await stateService.CreateLongRunningOperation(operationId, upn);
            return new OkObjectResult(createdLongRunningOperation);
        }

        /// <summary>
        /// Updates a long-running operation and creates a log entry.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <param name="operation">The long-running operation entry to update.</param>
        /// <returns></returns>
        [HttpPut("{operationId}")]
        public async Task<IActionResult> UpdateLongRunningOperation(string instanceId, string operationId, [FromBody] LongRunningOperation operation)
        {
            if (operationId != operation.Id)
            {
                return BadRequest("The operation ID in the request path does not match the operation ID of the object in the request body.");
            }
            var newOperation = await stateService.UpsertLongRunningOperation(operation);
            return new OkObjectResult(newOperation);
        }

        /// <summary>
        /// Retrieves the result of a long-running operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <returns></returns>
        [HttpGet("{operationId}/result")]
        public async Task<IActionResult> GetLongRunningOperationResult(string instanceId, string operationId)
        {
            var operation = await stateService.GetLongRunningOperationResult(operationId);
            if (operation == null)
            {
                return NotFound();
            }

            return Ok(operation);
        }

        /// <summary>
        /// Inserts the result of a long-running operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="operationId">The long-running operation identifier.</param>
        /// <param name="operationResult">The operation result to insert.</param>
        /// <returns></returns>
        [HttpPost("{operationId}/result")]
        public async Task<IActionResult> CreateLongRunningOperationResult(string instanceId, string operationId, [FromBody] JsonElement operationResult)
        {
            dynamic? dynamicOperationResult = JsonSerializer.Deserialize<ExpandoObject>(operationResult.GetRawText());
            if (dynamicOperationResult == null)
            {
                return BadRequest("The operation result is not in the correct format.");
            }

            // Ensure operation_id is extracted correctly as a string.
            if (dynamicOperationResult.operation_id is JsonElement {ValueKind: JsonValueKind.String} jsonElement)
            {
                var operationIdFromBody = jsonElement.GetString();
                if (operationIdFromBody != operationId)
                {
                    return BadRequest("The operation ID in the request path does not match the operation ID of the object in the request body.");
                }
            }
            else
            {
                return BadRequest("The operation result does not contain a valid operation_id.");
            }

            var result = await stateService.UpsertLongRunningOperationResult(dynamicOperationResult);
            return new OkObjectResult(result);
        }
    }
}
