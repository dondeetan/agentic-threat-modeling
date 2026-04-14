from __future__ import annotations
from typing import Protocol
from src.app.core.config import settings

class StoreProtocol(Protocol):
    def create_submission(self, doc: dict) -> dict: ...
    def get_submission(self, submission_id: str, tenant_id: str) -> dict | None: ...
    def create_run(self, doc: dict) -> dict: ...
    def get_run(self, run_id: str, tenant_id: str) -> dict | None: ...

class InMemoryStore:
    def __init__(self) -> None:
        self.submissions: dict[tuple[str, str], dict] = {}
        self.runs: dict[tuple[str, str], dict] = {}

    def create_submission(self, doc: dict) -> dict:
        self.submissions[(doc["tenantId"], doc["id"])] = doc
        return doc

    def get_submission(self, submission_id: str, tenant_id: str) -> dict | None:
        return self.submissions.get((tenant_id, submission_id))

    def create_run(self, doc: dict) -> dict:
        self.runs[(doc["tenantId"], doc["id"])] = doc
        return doc

    def get_run(self, run_id: str, tenant_id: str) -> dict | None:
        return self.runs.get((tenant_id, run_id))

_store: StoreProtocol | None = None

def get_store() -> StoreProtocol:
    global _store
    if _store is None:
        if settings.use_in_memory_store:
            _store = InMemoryStore()
        else:
            from src.app.integrations.cosmos import CosmosStore
            _store = CosmosStore()
    return _store