namespace ThreatModeler.Models;

public sealed class Submission
{
    public string Id { get; set; } = $"sub-{Guid.NewGuid()}";
    public string TenantId { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string ArchitectureSummary { get; set; } = string.Empty;
    public List<string> Components { get; set; } = new();
    public List<string> DataFlows { get; set; } = new();
    public List<string> TrustBoundaries { get; set; } = new();
    public string AuthenticationDetails { get; set; } = string.Empty;
    public List<string> SensitiveData { get; set; } = new();
    public string InternetExposure { get; set; } = string.Empty;
    public List<string> ExistingControls { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
    public string Status { get; set; } = "submitted";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class ThreatModelRun
{
    public string Id { get; set; } = $"run-{Guid.NewGuid()}";
    public string TenantId { get; set; } = string.Empty;
    public string SubmissionId { get; set; } = string.Empty;
    public string AnalyzerType { get; set; } = string.Empty;
    public string Status { get; set; } = "completed";
    public object Result { get; set; } = new { };
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
