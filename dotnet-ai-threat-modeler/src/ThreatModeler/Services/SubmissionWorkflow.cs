using ThreatModeler.Models;

namespace ThreatModeler.Services;

public interface ISubmissionWorkflow
{
    Task<SubmissionCreatedResponse> CreateSubmissionAsync(SubmissionRequest request, CancellationToken cancellationToken);
    Task<RunCreatedResponse?> AnalyzeSubmissionAsync(string submissionId, string tenantId, CancellationToken cancellationToken);
    Task<ThreatModelRun?> GetResultsAsync(string runId, string tenantId, CancellationToken cancellationToken);
}

public sealed class SubmissionWorkflow(ISubmissionStore store, IAnalyzerFactory analyzerFactory, string analyzerType) : ISubmissionWorkflow
{
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

        return new SubmissionCreatedResponse(submission.Id, submission.TenantId, submission.Status);
    }

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

        return new RunCreatedResponse(run.Id, submissionId, run.Status);
    }

    public Task<ThreatModelRun?> GetResultsAsync(
        string runId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        // Facade: the API layer uses this single workflow instead of coordinating stores and analyzers.
        return store.GetRunAsync(tenantId, runId, cancellationToken);
    }
}

public sealed record SubmissionCreatedResponse(string Id, string TenantId, string Status);

public sealed record RunCreatedResponse(string RunId, string SubmissionId, string Status);
