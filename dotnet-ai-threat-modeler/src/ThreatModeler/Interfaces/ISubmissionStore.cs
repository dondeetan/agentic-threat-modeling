using ThreatModeler.Models;

namespace ThreatModeler.Interfaces;

/// <summary>
/// Persists submissions and threat model analysis runs behind a repository boundary.
/// </summary>
public interface ISubmissionStore
{
    /// <summary>
    /// Stores a new submission.
    /// </summary>
    /// <param name="submission">The submission to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the persistence operation.</param>
    /// <returns>The persisted submission.</returns>
    Task<Submission> CreateSubmissionAsync(Submission submission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a submission by tenant and submission identifier.
    /// </summary>
    /// <param name="tenantId">The tenant that owns the submission.</param>
    /// <param name="submissionId">The submission identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the lookup.</param>
    /// <returns>The matching submission, or null when it does not exist.</returns>
    Task<Submission?> GetSubmissionAsync(string tenantId, string submissionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a completed threat model analysis run.
    /// </summary>
    /// <param name="run">The analysis run to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the persistence operation.</param>
    /// <returns>The persisted analysis run.</returns>
    Task<ThreatModelRun> CreateRunAsync(ThreatModelRun run, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an analysis run by tenant and run identifier.
    /// </summary>
    /// <param name="tenantId">The tenant that owns the analysis run.</param>
    /// <param name="runId">The analysis run identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the lookup.</param>
    /// <returns>The matching analysis run, or null when it does not exist.</returns>
    Task<ThreatModelRun?> GetRunAsync(string tenantId, string runId, CancellationToken cancellationToken = default);
}
