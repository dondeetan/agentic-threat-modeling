# Cosmos DB schema for the MVP

Use Cosmos DB as the document database for:
- submissions
- threat model runs
- reviews
- work item mappings

Recommended partition key:
- `/tenantId`

Suggested containers:
- `submissions`
- `threatModelRuns`
- `reviews`
- `workItemLinks`
