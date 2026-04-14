using Azure.Cosmos;
using ThreatModeler.Configuration;
using ThreatModeler.Models;

namespace ThreatModeler.Services;

public sealed class CosmosSubmissionStore : ISubmissionStore
{
    private readonly Container _submissions;
    private readonly Container _runs;

    public CosmosSubmissionStore(CosmosOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.Key))
        {
            throw new InvalidOperationException("Cosmos DB configuration is missing.");
        }

        var client = new CosmosClient(options.Endpoint, options.Key);
        var database = client.CreateDatabaseIfNotExistsAsync(options.Database).GetAwaiter().GetResult().Database;
        _submissions = database.CreateContainerIfNotExistsAsync(options.SubmissionsContainer, "/tenantId").GetAwaiter().GetResult().Container;
        _runs = database.CreateContainerIfNotExistsAsync(options.RunsContainer, "/tenantId").GetAwaiter().GetResult().Container;
    }

    public async Task<Submission> CreateSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        await _submissions.CreateItemAsync(submission, new PartitionKey(submission.TenantId), cancellationToken: cancellationToken);
        return submission;
    }

    public async Task<Submission?> GetSubmissionAsync(string tenantId, string submissionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _submissions.ReadItemAsync<Submission>(submissionId, new PartitionKey(tenantId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ThreatModelRun> CreateRunAsync(ThreatModelRun run, CancellationToken cancellationToken = default)
    {
        await _runs.CreateItemAsync(run, new PartitionKey(run.TenantId), cancellationToken: cancellationToken);
        return run;
    }

    public async Task<ThreatModelRun?> GetRunAsync(string tenantId, string runId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _runs.ReadItemAsync<ThreatModelRun>(runId, new PartitionKey(tenantId), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch
        {
            return null;
        }
    }
}
