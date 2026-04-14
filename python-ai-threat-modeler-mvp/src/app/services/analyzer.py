from __future__ import annotations
from typing import Protocol
from src.app.core.config import settings

class AnalyzerProtocol(Protocol):
    analyzer_type: str
    def analyze(self, submission: dict) -> dict: ...

class MockAnalyzer:
    analyzer_type = "mock"

    def analyze(self, submission: dict) -> dict:
        app_name = submission["applicationName"]
        components = submission.get("components", [])
        trust_boundaries = submission.get("trustBoundaries", [])
        return {
            "summary": f"{app_name} exposes typical web/API threats and should prioritize identity, trust boundary, and data protection controls.",
            "assets": [
                {"name": c, "type": "component", "sensitivity": "medium"} for c in components
            ],
            "trustBoundaries": trust_boundaries,
            "threats": [
                {
                    "id": "TM-001",
                    "strideCategory": "Spoofing",
                    "component": components[0] if components else "Unknown",
                    "threatStatement": "Identity tokens could be spoofed or replayed if token validation is weak.",
                    "mitigations": ["Entra ID", "Validate issuer/audience", "Short token lifetimes"],
                },
                {
                    "id": "TM-002",
                    "strideCategory": "Tampering",
                    "component": components[-1] if components else "Unknown",
                    "threatStatement": "Data could be modified in transit or through over-privileged service access.",
                    "mitigations": ["Managed Identity", "Private Endpoints", "Least privilege RBAC"],
                },
                {
                    "id": "TM-003",
                    "strideCategory": "Information Disclosure",
                    "component": components[-1] if components else "Unknown",
                    "threatStatement": "Sensitive data may be exposed through logs, misconfigured storage, or overly broad query access.",
                    "mitigations": ["Data classification", "Log scrubbing", "Encryption at rest/in transit"],
                },
            ],
            "topPriorities": [
                "Enforce managed identity and least privilege.",
                "Add private connectivity to data services.",
                "Harden logging to avoid sensitive data leakage.",
            ],
        }

_analyzer: AnalyzerProtocol | None = None

def get_analyzer() -> AnalyzerProtocol:
    global _analyzer
    if _analyzer is None:
        if settings.use_mock_analyzer:
            _analyzer = MockAnalyzer()
        else:
            from src.app.integrations.azure_openai import AzureOpenAIAnalyzer
            _analyzer = AzureOpenAIAnalyzer()
    return _analyzer