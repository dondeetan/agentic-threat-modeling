from src.app.services.storage import InMemoryStore


def test_get_submission_uses_tenant_and_submission_key():
    store = InMemoryStore()
    submission = {"tenantId": "tenant-a", "id": "sub-123", "applicationName": "Claims API"}

    store.create_submission(submission)

    assert store.get_submission("sub-123", "tenant-a")["applicationName"] == "Claims API"
    assert store.get_submission("sub-123", "tenant-b") is None


def test_get_run_uses_tenant_and_run_key():
    store = InMemoryStore()
    run = {"tenantId": "tenant-a", "id": "run-123", "submissionId": "sub-123", "analyzerType": "mock"}

    store.create_run(run)

    assert store.get_run("run-123", "tenant-a")["submissionId"] == "sub-123"
    assert store.get_run("run-123", "tenant-b") is None
