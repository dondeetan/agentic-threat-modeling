using ThreatModeler.Interfaces;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

/// <summary>
/// Coordinates submission persistence, analyzer selection, and result retrieval for API callers.
/// </summary>
public sealed class SubmissionWorkflow(ISubmissionStore store, IAnalyzerFactory analyzerFactory, string analyzerType) : ISubmissionWorkflow
{
    /// <inheritdoc />
    public async Task<SubmissionCreatedResponse> CreateSubmissionAsync(
        SubmissionRequest request,
        CancellationToken cancellationToken)
    {
        // Builder-style mapping: the DTO is translated into a domain submission in one cohesive step.
        var submission = new Submission
        {
            TenantId = request.TenantId,
            ApplicationName = request.ApplicationName,
            BusinessPurpose = request.BusinessPurpose,
            ArchitectureSummary = request.ArchitectureSummary,
            Components = request.Components,
            DataFlows = request.DataFlows,
            TrustBoundaries = request.TrustBoundaries,
            OpenApiDocument = request.OpenApiDocument,
            AuthenticationDetails = request.AuthenticationDetails,
            SensitiveData = request.SensitiveData,
            InternetExposure = request.InternetExposure,
            ExistingControls = request.ExistingControls,
            Assumptions = request.Assumptions
        };

        await store.CreateSubmissionAsync(submission, cancellationToken);

        // Data Transfer Object pattern: return only the API-facing status instead of the full domain object.
        return new SubmissionCreatedResponse(submission.Id, submission.TenantId, submission.Status);
    }

    /// <inheritdoc />
    public async Task<RunCreatedResponse?> AnalyzeSubmissionAsync(
        string submissionId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        var submission = await store.GetSubmissionAsync(tenantId, submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        // Factory Method + Strategy: select the configured analyzer, then run it through the common contract.
        var analyzer = analyzerFactory.Create(analyzerType);
        var result = await analyzer.AnalyzeAsync(submission, cancellationToken);
        var run = new ThreatModelRun
        {
            TenantId = tenantId,
            SubmissionId = submissionId,
            AnalyzerType = analyzer.AnalyzerType,
            Result = result
        };

        await store.CreateRunAsync(run, cancellationToken);

        // Data Transfer Object pattern: expose identifiers and status while the result remains retrievable separately.
        return new RunCreatedResponse(run.Id, submissionId, run.Status);
    }

    /// <inheritdoc />
    public Task<ThreatModelRun?> GetResultsAsync(
        string runId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        // Facade: the API layer uses this single workflow instead of coordinating stores and analyzers.
        return store.GetRunAsync(tenantId, runId, cancellationToken);
    }
}

/// <summary>
/// Response returned after a submission is accepted.
/// </summary>
public sealed record SubmissionCreatedResponse(string Id, string TenantId, string Status);

/// <summary>
/// Response returned after a threat model analysis run is created.
/// </summary>
public sealed record RunCreatedResponse(string RunId, string SubmissionId, string Status);
