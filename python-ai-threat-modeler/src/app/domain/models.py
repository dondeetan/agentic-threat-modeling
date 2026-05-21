from __future__ import annotations
from dataclasses import asdict, dataclass, field
from datetime import datetime, timezone
from typing import Any
import uuid


def utcnow() -> str:
    return datetime.now(timezone.utc).isoformat()


@dataclass
class Submission:
    tenantId: str
    applicationName: str
    businessPurpose: str
    architectureSummary: str
    components: list[str]
    dataFlows: list[str]
    trustBoundaries: list[str]
    openApiDocument: str
    authenticationDetails: str
    sensitiveData: list[str]
    internetExposure: str
    existingControls: list[str]
    assumptions: list[str]
    id: str = field(default_factory=lambda: f"sub-{uuid.uuid4()}")
    createdAt: str = field(default_factory=utcnow)
    status: str = "submitted"

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)

    def to_prompt_dict(self) -> dict[str, Any]:
        # Adapter-style normalization: prompt payloads match the .NET domain property names.
        return {
            "Id": self.id,
            "TenantId": self.tenantId,
            "ApplicationName": self.applicationName,
            "BusinessPurpose": self.businessPurpose,
            "ArchitectureSummary": self.architectureSummary,
            "Components": self.components,
            "DataFlows": self.dataFlows,
            "TrustBoundaries": self.trustBoundaries,
            "OpenApiDocument": self.openApiDocument,
            "AuthenticationDetails": self.authenticationDetails,
            "SensitiveData": self.sensitiveData,
            "InternetExposure": self.internetExposure,
            "ExistingControls": self.existingControls,
            "Assumptions": self.assumptions,
            "Status": self.status,
            "CreatedAt": self.createdAt,
        }

    @classmethod
    def from_dict(cls, value: dict[str, Any]) -> "Submission":
        return cls(**value)


@dataclass
class ThreatModelRun:
    tenantId: str
    submissionId: str
    analyzerType: str
    result: dict[str, Any]
    id: str = field(default_factory=lambda: f"run-{uuid.uuid4()}")
    createdAt: str = field(default_factory=utcnow)
    status: str = "completed"

    def to_dict(self) -> dict[str, Any]:
        return asdict(self)
