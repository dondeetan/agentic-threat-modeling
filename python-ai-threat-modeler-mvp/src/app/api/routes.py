from fastapi import APIRouter, HTTPException, Query
from src.app.domain.models import Submission, ThreatModelRun
from src.app.domain.schemas import SubmissionCreate, SubmissionResponse, AnalyzeResponse, ResultResponse
from src.app.services.storage import get_store
from src.app.services.analyzer import get_analyzer

router = APIRouter()

@router.get("/health")
def health() -> dict:
    return {"status": "ok"}

@router.post("/submit", response_model=SubmissionResponse)
def submit_workload(payload: SubmissionCreate) -> SubmissionResponse:
    submission = Submission(**payload.model_dump())
    get_store().create_submission(submission.to_dict())
    return SubmissionResponse(id=submission.id, tenantId=submission.tenantId, status=submission.status)

@router.post("/analyze/{submission_id}", response_model=AnalyzeResponse)
def analyze_submission(submission_id: str, tenant_id: str = Query(..., alias="tenantId")) -> AnalyzeResponse:
    submission = get_store().get_submission(submission_id=submission_id, tenant_id=tenant_id)
    if not submission:
        raise HTTPException(status_code=404, detail="Submission not found.")

    analyzer = get_analyzer()
    result = analyzer.analyze(submission)
    run = ThreatModelRun(
        tenantId=tenant_id,
        submissionId=submission_id,
        analyzerType=analyzer.analyzer_type,
        result=result,
    )
    get_store().create_run(run.to_dict())
    return AnalyzeResponse(runId=run.id, submissionId=submission_id, status=run.status)

@router.get("/results/{run_id}", response_model=ResultResponse)
def get_results(run_id: str, tenant_id: str = Query(..., alias="tenantId")) -> ResultResponse:
    run = get_store().get_run(run_id=run_id, tenant_id=tenant_id)
    if not run:
        raise HTTPException(status_code=404, detail="Run not found.")
    return ResultResponse(
        id=run["id"],
        submissionId=run["submissionId"],
        analyzerType=run["analyzerType"],
        status=run["status"],
        result=run["result"],
    )
