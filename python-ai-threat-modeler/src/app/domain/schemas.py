from pydantic import BaseModel, Field
from typing import Any


class SubmissionCreate(BaseModel):
    tenantId: str = Field(..., min_length=1)
    applicationName: str
    businessPurpose: str
    architectureSummary: str
    components: list[str]
    dataFlows: list[str]
    trustBoundaries: list[str]
    openApiDocument: str = ""
    authenticationDetails: str
    sensitiveData: list[str]
    internetExposure: str
    existingControls: list[str] = Field(default_factory=list)
    assumptions: list[str] = Field(default_factory=list)


class SubmissionResponse(BaseModel):
    id: str
    tenantId: str
    status: str


class AnalyzeResponse(BaseModel):
    runId: str
    submissionId: str
    status: str


class ResultResponse(BaseModel):
    id: str
    tenantId: str
    submissionId: str
    analyzerType: str
    status: str
    result: dict[str, Any]
    createdAt: str
