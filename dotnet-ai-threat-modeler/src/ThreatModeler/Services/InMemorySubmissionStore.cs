using System.Collections.Concurrent;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class InMemorySubmissionStore : ISubmissionStore
{
    // Repository pattern: storage mechanics stay hidden behind ISubmissionStore.
    private readonly ConcurrentDictionary<(string TenantId, string SubmissionId), Submission> _submissions = new();
    private readonly ConcurrentDictionary<(string TenantId, string RunId), ThreatModelRun> _runs = new();

    public Task<Submission> CreateSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        _submissions[(submission.TenantId, submission.Id)] = submission;
        return Task.FromResult(submission);
    }

    public Task<Submission?> GetSubmissionAsync(string tenantId, string submissionId, CancellationToken cancellationToken = default)
    {
        _submissions.TryGetValue((tenantId, submissionId), out var submission);
        return Task.FromResult(submission);
    }

    public Task<ThreatModelRun> CreateRunAsync(ThreatModelRun run, CancellationToken cancellationToken = default)
    {
        _runs[(run.TenantId, run.Id)] = run;
        return Task.FromResult(run);
    }

    public Task<ThreatModelRun?> GetRunAsync(string tenantId, string runId, CancellationToken cancellationToken = default)
    {
        _runs.TryGetValue((tenantId, runId), out var run);
        return Task.FromResult(run);
    }
}
