using ThreatModeler.Configuration;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class CosmosSubmissionStore : ISubmissionStore
{
    private readonly InMemorySubmissionStore _fallbackStore = new();
    private readonly bool _isConfigured;

    public CosmosSubmissionStore(CosmosOptions options)
    {
        _isConfigured =
            !string.IsNullOrWhiteSpace(options.Endpoint) &&
            !string.IsNullOrWhiteSpace(options.Key);
    }

    public async Task<Submission> CreateSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        return await _fallbackStore.CreateSubmissionAsync(submission, cancellationToken);
    }

    public async Task<Submission?> GetSubmissionAsync(string tenantId, string submissionId, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        return await _fallbackStore.GetSubmissionAsync(tenantId, submissionId, cancellationToken);
    }

    public async Task<ThreatModelRun> CreateRunAsync(ThreatModelRun run, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        return await _fallbackStore.CreateRunAsync(run, cancellationToken);
    }

    public async Task<ThreatModelRun?> GetRunAsync(string tenantId, string runId, CancellationToken cancellationToken = default)
    {
        EnsureSupported();
        return await _fallbackStore.GetRunAsync(tenantId, runId, cancellationToken);
    }

    private void EnsureSupported()
    {
        if (!_isConfigured)
        {
            throw new InvalidOperationException(
                "Cosmos DB settings are missing. Run with App:UseInMemoryStore=true for local development.");
        }

        throw new NotSupportedException(
            "This scaffold currently builds without the Azure Cosmos SDK. Add the package and implementation when enabling live Cosmos persistence.");
    }
}
