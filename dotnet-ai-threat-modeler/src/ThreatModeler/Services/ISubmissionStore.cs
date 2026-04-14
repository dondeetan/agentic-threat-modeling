using ThreatModeler.Models;

namespace ThreatModeler.Services;

public interface ISubmissionStore
{
    Task<Submission> CreateSubmissionAsync(Submission submission, CancellationToken cancellationToken = default);
    Task<Submission?> GetSubmissionAsync(string tenantId, string submissionId, CancellationToken cancellationToken = default);
    Task<ThreatModelRun> CreateRunAsync(ThreatModelRun run, CancellationToken cancellationToken = default);
    Task<ThreatModelRun?> GetRunAsync(string tenantId, string runId, CancellationToken cancellationToken = default);
}
