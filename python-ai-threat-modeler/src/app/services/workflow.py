from __future__ import annotations

from dataclasses import dataclass
from typing import Any

from src.app.domain.models import Submission, ThreatModelRun
from src.app.domain.schemas import SubmissionCreate
from src.app.services.analyzer import AnalyzerFactory
from src.app.services.storage import StoreProtocol


@dataclass(frozen=True)
class SubmissionCreatedResponse:
    id: str
    tenantId: str
    status: str


@dataclass(frozen=True)
class RunCreatedResponse:
    runId: str
    submissionId: str
    status: str


class SubmissionWorkflow:
    def __init__(self, store: StoreProtocol, analyzer_factory: AnalyzerFactory, analyzer_type: str) -> None:
        self._store = store
        self._analyzer_factory = analyzer_factory
        self._analyzer_type = analyzer_type

    def create_submission(self, request: SubmissionCreate) -> SubmissionCreatedResponse:
        # Builder-style mapping: the DTO is translated into a domain submission in one cohesive step.
        submission = Submission(**request.model_dump())
        self._store.create_submission(submission.to_dict())

        # Data Transfer Object pattern: return only the API-facing status instead of the full domain object.
        return SubmissionCreatedResponse(submission.id, submission.tenantId, submission.status)

    def analyze_submission(self, submission_id: str, tenant_id: str) -> RunCreatedResponse | None:
        submission_doc = self._store.get_submission(submission_id=submission_id, tenant_id=tenant_id)
        if submission_doc is None:
            return None

        submission = Submission.from_dict(submission_doc)

        # Factory Method + Strategy: select the configured analyzer, then run it through the common contract.
        analyzer = self._analyzer_factory.create(self._analyzer_type)
        result = analyzer.analyze(submission)
        run = ThreatModelRun(
            tenantId=tenant_id,
            submissionId=submission_id,
            analyzerType=analyzer.analyzer_type,
            result=result,
        )

        self._store.create_run(run.to_dict())

        # Data Transfer Object pattern: expose identifiers and status while the result remains retrievable separately.
        return RunCreatedResponse(run.id, submission_id, run.status)

    def get_results(self, run_id: str, tenant_id: str) -> dict[str, Any] | None:
        # Facade: the API layer uses this single workflow instead of coordinating stores and analyzers.
        return self._store.get_run(run_id=run_id, tenant_id=tenant_id)

