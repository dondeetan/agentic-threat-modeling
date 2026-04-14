using ThreatModeler.Models;

namespace ThreatModeler.Services;

public static class SubmissionWorkflow
{
    public static async Task<SubmissionCreatedResponse> CreateSubmissionAsync(
        SubmissionRequest request,
        ISubmissionStore store,
        CancellationToken cancellationToken)
    {
        var submission = new Submission
        {
            TenantId = request.TenantId,
            ApplicationName = request.ApplicationName,
            BusinessPurpose = request.BusinessPurpose,
            ArchitectureSummary = request.ArchitectureSummary,
            Components = request.Components,
            DataFlows = request.DataFlows,
            TrustBoundaries = request.TrustBoundaries,
            AuthenticationDetails = request.AuthenticationDetails,
            SensitiveData = request.SensitiveData,
            InternetExposure = request.InternetExposure,
            ExistingControls = request.ExistingControls,
            Assumptions = request.Assumptions
        };

        await store.CreateSubmissionAsync(submission, cancellationToken);

        return new SubmissionCreatedResponse(submission.Id, submission.TenantId, submission.Status);
    }

    public static async Task<RunCreatedResponse?> AnalyzeSubmissionAsync(
        string submissionId,
        string tenantId,
        ISubmissionStore store,
        IAnalyzer analyzer,
        CancellationToken cancellationToken)
    {
        var submission = await store.GetSubmissionAsync(tenantId, submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

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

    public static Task<ThreatModelRun?> GetResultsAsync(
        string runId,
        string tenantId,
        ISubmissionStore store,
        CancellationToken cancellationToken)
    {
        return store.GetRunAsync(tenantId, runId, cancellationToken);
    }
}

public sealed record SubmissionCreatedResponse(string Id, string TenantId, string Status);

public sealed record RunCreatedResponse(string RunId, string SubmissionId, string Status);
