using ThreatModeler.Models;
using ThreatModeler.Services;

namespace ThreatModeler.Interfaces;

/// <summary>
/// Defines the application workflow for creating submissions, analyzing them, and reading results.
/// </summary>
public interface ISubmissionWorkflow
{
    /// <summary>
    /// Creates and persists a submission from an API request.
    /// </summary>
    /// <param name="request">The incoming submission request.</param>
    /// <param name="cancellationToken">A token used to cancel the workflow operation.</param>
    /// <returns>A response containing the new submission identifier and status.</returns>
    Task<SubmissionCreatedResponse> CreateSubmissionAsync(SubmissionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Runs analysis for an existing submission.
    /// </summary>
    /// <param name="submissionId">The submission identifier to analyze.</param>
    /// <param name="tenantId">The tenant that owns the submission.</param>
    /// <param name="cancellationToken">A token used to cancel the workflow operation.</param>
    /// <returns>A response containing the new run identifier, or null when the submission is not found.</returns>
    Task<RunCreatedResponse?> AnalyzeSubmissionAsync(string submissionId, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a stored analysis run.
    /// </summary>
    /// <param name="runId">The analysis run identifier.</param>
    /// <param name="tenantId">The tenant that owns the run.</param>
    /// <param name="cancellationToken">A token used to cancel the lookup.</param>
    /// <returns>The matching analysis run, or null when the run is not found.</returns>
    Task<ThreatModelRun?> GetResultsAsync(string runId, string tenantId, CancellationToken cancellationToken);
}
