using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class HealthFunctions
{
    private readonly ILogger<HealthFunctions> _logger;

    public HealthFunctions(ILogger<HealthFunctions> logger)
    {
        _logger = logger;
    }

    [Function("Health")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        _logger.LogInformation("Health check requested");

        // custom manual check
        bool healthy = true;

        var response = req.CreateResponse(
            healthy ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable);

        response.Headers.Add("Content-Type", "application/json");

        await response.WriteStringAsync("{\"status\":\"healthy\"}");

        return response;
    }
}
