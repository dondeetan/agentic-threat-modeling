from __future__ import annotations
from azure.cosmos import CosmosClient, PartitionKey
from src.app.core.config import settings

class CosmosStore:
    def __init__(self) -> None:
        if not settings.cosmos_endpoint or not settings.cosmos_key:
            raise ValueError("Cosmos DB configuration is missing.")
        self.client = CosmosClient(settings.cosmos_endpoint, credential=settings.cosmos_key)
        self.db = self.client.create_database_if_not_exists(id=settings.cosmos_database)
        self.submissions = self.db.create_container_if_not_exists(
            id=settings.cosmos_submissions_container,
            partition_key=PartitionKey(path="/tenantId"),
        )
        self.runs = self.db.create_container_if_not_exists(
            id=settings.cosmos_runs_container,
            partition_key=PartitionKey(path="/tenantId"),
        )

    def create_submission(self, doc: dict) -> dict:
        return self.submissions.create_item(body=doc)

    def get_submission(self, submission_id: str, tenant_id: str) -> dict | None:
        try:
            return self.submissions.read_item(item=submission_id, partition_key=tenant_id)
        except Exception:
            return None

    def create_run(self, doc: dict) -> dict:
        return self.runs.create_item(body=doc)

    def get_run(self, run_id: str, tenant_id: str) -> dict | None:
        try:
            return self.runs.read_item(item=run_id, partition_key=tenant_id)
        except Exception:
            return None
