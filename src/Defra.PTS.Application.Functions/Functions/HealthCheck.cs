using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Defra.PTS.Application.Api.Services.Interface;

namespace Defra.PTS.Functions.Functions
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <remarks>
    /// Health check dependancies
    /// </remarks>
    /// <param name="applicationService"></param>
    /// <param name="logger"></param>
    public class HealthCheck(IApplicationService applicationService, ILogger<HealthCheck> logger)
    {
        private readonly IApplicationService _applicationService = applicationService;
        private readonly ILogger<HealthCheck> _logger = logger;

        /// <summary>
        /// Check service health
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("HealthCheck")]
        [OpenApiOperation(operationId: "Run", tags: "name")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
#pragma warning disable IDE0060 // Remove unused parameter
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger.LogInformation("Health Check Trigger.");

            // Perform health check logic here
            bool isHealthy = await _applicationService.PerformHealthCheckLogic();

            if (isHealthy)
            {
                return new OkResult();
            }
            else
            {
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
