from src.app.domain.models import Submission, ThreatModelRun
from src.app.domain.schemas import SubmissionCreate
from src.app.services.analyzer import AnalyzerFactory, MockAnalyzer
from src.app.services.storage import InMemoryStore
from src.app.services.workflow import SubmissionWorkflow


def create_request() -> SubmissionCreate:
    return SubmissionCreate(
        tenantId="tenant-a",
        applicationName="Claims API",
        businessPurpose="Process claims",
        architectureSummary="SPA to API to data store",
        components=["React SPA", "API", "Storage"],
        dataFlows=["SPA to API", "API to Storage"],
        trustBoundaries=["Internet", "App to Data"],
        openApiDocument='{"openapi":"3.0.1"}',
        authenticationDetails="Entra ID",
        sensitiveData=["PII"],
        internetExposure="Public API",
        existingControls=["WAF"],
        assumptions=["No public DB access"],
    )


def create_workflow(store: InMemoryStore, analyzer_type: str = "mock") -> SubmissionWorkflow:
    return SubmissionWorkflow(store, AnalyzerFactory([MockAnalyzer()]), analyzer_type)


def test_create_submission_persists_submission_and_returns_status():
    store = InMemoryStore()
    request = create_request()
    workflow = create_workflow(store)

    response = workflow.create_submission(request)

    assert response.tenantId == request.tenantId
    assert response.status == "submitted"
    assert response.id
    stored = store.get_submission(response.id, request.tenantId)
    assert stored["applicationName"] == request.applicationName
    assert stored["components"] == request.components


def test_analyze_submission_returns_none_when_submission_does_not_exist():
    result = create_workflow(InMemoryStore()).analyze_submission("missing-submission", "tenant-a")

    assert result is None


def test_analyze_submission_creates_run_and_returns_status():
    store = InMemoryStore()
    workflow = create_workflow(store)
    submission = Submission(
        tenantId="tenant-a",
        applicationName="Claims API",
        businessPurpose="Process claims",
        architectureSummary="SPA to API",
        components=["React SPA", "API"],
        dataFlows=[],
        trustBoundaries=["Internet to App"],
        openApiDocument="{}",
        authenticationDetails="",
        sensitiveData=[],
        internetExposure="",
        existingControls=[],
        assumptions=[],
    )
    store.create_submission(submission.to_dict())

    response = workflow.analyze_submission(submission.id, submission.tenantId)

    assert response is not None
    assert response.submissionId == submission.id
    assert response.status == "completed"
    stored_run = store.get_run(response.runId, submission.tenantId)
    assert stored_run["analyzerType"] == "mock"
    assert stored_run["submissionId"] == submission.id


def test_get_results_returns_stored_run():
    store = InMemoryStore()
    workflow = create_workflow(store)
    run = ThreatModelRun(
        tenantId="tenant-a",
        submissionId="sub-123",
        analyzerType="mock",
        result={"summary": "ok"},
    )
    store.create_run(run.to_dict())

    response = workflow.get_results(run.id, run.tenantId)

    assert response["id"] == run.id
    assert response["submissionId"] == run.submissionId
    assert response["analyzerType"] == run.analyzerType
