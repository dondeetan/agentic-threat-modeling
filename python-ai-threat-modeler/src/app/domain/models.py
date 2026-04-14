from __future__ import annotations
from dataclasses import dataclass, field
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
    authenticationDetails: str
    sensitiveData: list[str]
    internetExposure: str
    existingControls: list[str]
    assumptions: list[str]
    id: str = field(default_factory=lambda: f"sub-{uuid.uuid4()}")
    createdAt: str = field(default_factory=utcnow)
    status: str = "submitted"

    def to_dict(self) -> dict[str, Any]:
        return self.__dict__.copy()

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
        return self.__dict__.copy()
