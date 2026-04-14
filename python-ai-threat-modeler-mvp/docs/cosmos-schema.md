# Cosmos DB schema for the MVP

## Purpose

The MVP uses Cosmos DB as the document database and system of record for:
- submissions
- threat model runs
- review decisions
- work item mappings

## Containers

### 1) submissions
Stores the original threat-modeling input.

Suggested partition key:
- `/tenantId`

Example fields:
- `id`
- `tenantId`
- `applicationName`
- `businessPurpose`
- `architectureSummary`
- `components`
- `dataFlows`
- `trustBoundaries`
- `authenticationDetails`
- `sensitiveData`
- `internetExposure`
- `existingControls`
- `assumptions`
- `status`
- `createdAt`

### 2) threatModelRuns
Stores the result from the analyzer.

Suggested partition key:
- `/tenantId`

Example fields:
- `id`
- `tenantId`
- `submissionId`
- `analyzerType`
- `status`
- `result`
- `createdAt`

### 3) reviews
Stores security architect approval or comments.

### 4) workItemLinks
Maps threats or runs to Azure DevOps / GitHub work items.
