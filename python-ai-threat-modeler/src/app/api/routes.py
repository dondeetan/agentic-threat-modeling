from fastapi import APIRouter, HTTPException, Query
from src.app.domain.schemas import SubmissionCreate, SubmissionResponse, AnalyzeResponse, ResultResponse
from src.app.core.config import settings
from src.app.services.analyzer import get_analyzer_factory
from src.app.services.storage import get_store
from src.app.services.workflow import SubmissionWorkflow

router = APIRouter()


def get_workflow() -> SubmissionWorkflow:
    analyzer_type = "mock" if settings.use_mock_analyzer else settings.analyzer_type
    return SubmissionWorkflow(get_store(), get_analyzer_factory(), analyzer_type)


@router.get("/health")
def health() -> dict:
    return {"status": "ok"}


@router.post("/submit", response_model=SubmissionResponse)
def submit_workload(payload: SubmissionCreate) -> SubmissionResponse:
    # Facade pattern: the route delegates orchestration to one workflow boundary.
    response = get_workflow().create_submission(payload)
    return SubmissionResponse(id=response.id, tenantId=response.tenantId, status=response.status)


@router.post("/analyze/{submission_id}", response_model=AnalyzeResponse)
def analyze_submission(submission_id: str, tenant_id: str = Query(..., alias="tenantId")) -> AnalyzeResponse:
    # Dependency Inversion Principle: the route depends on workflow/analyzer abstractions, not concrete services.
    response = get_workflow().analyze_submission(submission_id, tenant_id)
    if response is None:
        raise HTTPException(status_code=404, detail="Submission not found.")
    return AnalyzeResponse(runId=response.runId, submissionId=response.submissionId, status=response.status)


@router.get("/results/{run_id}", response_model=ResultResponse)
def get_results(run_id: str, tenant_id: str = Query(..., alias="tenantId")) -> ResultResponse:
    # Single Responsibility Principle: HTTP concerns stay here while analysis/persistence stay in services.
    response = get_workflow().get_results(run_id, tenant_id)
    if response is None:
        raise HTTPException(status_code=404, detail="Run not found.")
    return ResultResponse(
        id=response["id"],
        tenantId=response["tenantId"],
        submissionId=response["submissionId"],
        analyzerType=response["analyzerType"],
        status=response["status"],
        result=response["result"],
        createdAt=response["createdAt"],
    )
