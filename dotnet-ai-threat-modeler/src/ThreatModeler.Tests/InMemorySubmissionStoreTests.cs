using ThreatModeler.Models;
using ThreatModeler.Services;
using Xunit;

namespace ThreatModeler.Tests;

public sealed class InMemorySubmissionStoreTests
{
    [Fact]
    public async Task GetSubmissionAsync_UsesTenantAndSubmissionKey()
    {
        var store = new InMemorySubmissionStore();
        var submission = new Submission
        {
            TenantId = "tenant-a",
            ApplicationName = "Claims API"
        };

        await store.CreateSubmissionAsync(submission, CancellationToken.None);

        var found = await store.GetSubmissionAsync("tenant-a", submission.Id, CancellationToken.None);
        var missingForOtherTenant = await store.GetSubmissionAsync("tenant-b", submission.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal("Claims API", found!.ApplicationName);
        Assert.Null(missingForOtherTenant);
    }

    [Fact]
    public async Task GetRunAsync_UsesTenantAndRunKey()
    {
        var store = new InMemorySubmissionStore();
        var run = new ThreatModelRun
        {
            TenantId = "tenant-a",
            SubmissionId = "sub-123",
            AnalyzerType = "mock"
        };

        await store.CreateRunAsync(run, CancellationToken.None);

        var found = await store.GetRunAsync("tenant-a", run.Id, CancellationToken.None);
        var missingForOtherTenant = await store.GetRunAsync("tenant-b", run.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal("sub-123", found!.SubmissionId);
        Assert.Null(missingForOtherTenant);
    }
}
