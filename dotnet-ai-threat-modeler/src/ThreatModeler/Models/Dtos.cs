namespace ThreatModeler.Models;

public sealed class SubmissionRequest
{
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
}
