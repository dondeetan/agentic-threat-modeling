using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThreatModeler.Configuration;
using ThreatModeler.Models;
using ThreatModeler.Services;

namespace Api.Controllers;

[ApiController]
[Route("")]
public sealed class ThreatModelerController(
    ISubmissionWorkflow workflow,
    IOptions<AppOptions> appOptions) : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }

    [HttpGet("openapi/v1.json")]
    public IActionResult GetOpenApiDocument()
    {
        return Ok(new
        {
            openapi = "3.0.1",
            info = new { title = "OpenAPI Threat Modeler API", version = "v1" },
            paths = new Dictionary<string, object>
            {
                ["/health"] = new { get = new { summary = "Health probe" } },
                ["/submit"] = new { post = new { summary = "Submit application and OpenAPI context for threat modeling" } },
                ["/analyze/{submissionId}"] = new { post = new { summary = "Analyze a submitted threat model request" } },
                ["/results/{runId}"] = new { get = new { summary = "Get a completed threat model run" } }
            }
        });
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            service = "OpenAPI Threat Modeler API",
            defaultAnalyzer = appOptions.Value.AnalyzerType,
            endpoints = new[]
            {
                "GET /health",
                "GET /openapi/v1.json",
                "POST /submit",
                "POST /analyze/{submissionId}?tenantId={tenantId}",
                "GET /results/{runId}?tenantId={tenantId}"
            }
        });
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAsync(
        [FromBody] SubmissionRequest request,
        CancellationToken cancellationToken)
    {
        // Facade pattern: the controller delegates orchestration to one workflow boundary.
        var response = await workflow.CreateSubmissionAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("analyze/{submissionId}")]
    public async Task<IActionResult> AnalyzeAsync(
        string submissionId,
        [FromQuery] string tenantId,
        CancellationToken cancellationToken)
    {
        // Dependency Inversion Principle: the controller depends on ISubmissionWorkflow, not concrete services.
        var response = await workflow.AnalyzeSubmissionAsync(submissionId, tenantId, cancellationToken);
        return response is null
            ? NotFound(new { message = "Submission not found." })
            : Ok(response);
    }

    [HttpGet("results/{runId}")]
    public async Task<IActionResult> GetResultsAsync(
        string runId,
        [FromQuery] string tenantId,
        CancellationToken cancellationToken)
    {
        // Single Responsibility Principle: HTTP concerns stay here while analysis/persistence stay in services.
        var response = await workflow.GetResultsAsync(runId, tenantId, cancellationToken);
        return response is null
            ? NotFound(new { message = "Run not found." })
            : Ok(response);
    }
}
