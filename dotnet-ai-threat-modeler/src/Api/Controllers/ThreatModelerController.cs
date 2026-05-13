using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ThreatModeler.Configuration;
using ThreatModeler.Models;
using ThreatModeler.Services;

namespace Api.Controllers;

[ApiController]
[Route("[controller]/api")]
public sealed class ThreatModelerController(
    ISubmissionWorkflow workflow) : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
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
