# Python AI Threat Modeler MVP

A runnable FastAPI service for automated security threat modeling. The Python implementation mirrors the `dotnet-ai-threat-modeler` architecture: API routes delegate orchestration to a submission workflow, analyzers are selected through a factory, prompt context is loaded from local files, and persistence stays behind a repository contract.

## Design Approach

Follows the pattern references under `DotNet/Patterns` in [dondeetan/best-practices-poc](https://github.com/dondeetan/best-practices-poc):

- **Strategy pattern**: `AnalyzerProtocol` allows `ChatClientAnalyzer` and `MockAnalyzer` to be swapped without changing workflow code.
- **Factory Method pattern**: `AnalyzerFactory` selects the configured analyzer. `openai` is the default configured analyzer, while `USE_MOCK_ANALYZER=true` keeps local runs deterministic.
- **Facade pattern**: `SubmissionWorkflow` gives the API one application-service entry point for submit, analyze, and result retrieval.
- **Repository pattern**: `StoreProtocol` hides in-memory and Cosmos DB persistence.
- **Proxy pattern**: `CosmosStore` preserves the Cosmos-facing repository contract.
- **Provider pattern**: `FilePromptContextProvider` retrieves prompt rules, guidelines, and output format independently of analyzer transport.
- **Builder pattern**: `PromptBuilder` composes retrieved prompt context with a submitted system description.
- **SOLID principles**: workflow orchestration, analyzer selection, prompt retrieval, prompt composition, persistence, and route composition are separated so each type has a focused reason to change.

The code includes short comments at the implementation points where these patterns or principles are applied.

## Prompt Context Retrieval

`ChatClientAnalyzer` does not hardcode the full model prompt. It depends on:

- `FilePromptContextProvider`, which loads system rules, guideline fragments, and the required output schema from `src/app/prompts`.
- `PromptBuilder`, which assembles the retrieved context and serialized `Submission` into the final prompt.

The provider always loads baseline threat-modeling and STRIDE guidance. It conditionally includes cloud, identity, and data-protection guidance based on submitted components, data flows, trust boundaries, authentication details, sensitive data, and internet exposure.

## What This Repo Includes

- FastAPI API with endpoints:
  - `POST /submit`
  - `POST /analyze/{submission_id}?tenantId=...`
  - `GET /results/{run_id}?tenantId=...`
  - `GET /health`
- OpenAI, Azure OpenAI, and mock analyzers behind one factory
- File-backed prompt context retrieval for rules, guidelines, and output format
- Cosmos DB repository implementation with in-memory local store
- Tests for API flow, workflow behavior, store behavior, analyzer selection/output handling, prompt retrieval, and prompt composition

## Repository Structure

```text
python-ai-threat-modeler/
+-- README.md
+-- .env.example
+-- requirements.txt
+-- pyproject.toml
+-- src/
|   \-- app/
|       +-- main.py
|       +-- api/routes.py
|       +-- core/
|       +-- domain/
|       +-- integrations/
|       +-- prompts/
|       \-- services/
+-- tests/
+-- docs/
|   \-- cosmos-schema.md
\-- cosmos/
    \-- schema-examples.json
```

## Local Development

Prerequisites:

- Python 3.11+
- Optional Azure Cosmos DB account
- Optional OpenAI or Azure OpenAI deployment

Install dependencies:

```bash
pip install -r requirements.txt
```

Configure environment:

```bash
cp .env.example .env
```

Recommended local defaults:

```text
USE_MOCK_ANALYZER=true
USE_IN_MEMORY_STORE=true
ANALYZER_TYPE=openai
```

Supported analyzer types:

- `openai`
- `azure-openai`
- `mock`

When `ANALYZER_TYPE=openai`, set `OPENAI_API_KEY`, `OPENAI_ENDPOINT`, and `OPENAI_MODEL`.
When `ANALYZER_TYPE=azure-openai`, set the Azure OpenAI values in `.env`.

Run locally:

```bash
uvicorn src.app.main:app --reload
```

The API starts on `http://127.0.0.1:8000`.

## Example Flow

Submit a workload:

```bash
curl -X POST http://127.0.0.1:8000/submit \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "tenant-demo",
    "applicationName": "Claims API",
    "businessPurpose": "Processes insurance claims",
    "architectureSummary": "React SPA -> API Management -> App Service -> Azure SQL",
    "components": ["React SPA", "API Management", "App Service", "Azure SQL"],
    "dataFlows": ["Browser to SPA", "SPA to API Management", "App Service to Azure SQL"],
    "trustBoundaries": ["Internet to Azure Edge", "Application Tier to Data Tier"],
    "openApiDocument": "{\"openapi\":\"3.0.1\"}",
    "authenticationDetails": "Entra ID for users, Managed Identity for service-to-service",
    "sensitiveData": ["PII", "financial data"],
    "internetExposure": "Public SPA and API entrypoint",
    "existingControls": ["WAF", "Key Vault", "Defender for Cloud"],
    "assumptions": ["No public access to SQL"]
  }'
```

Analyze:

```bash
curl -X POST "http://127.0.0.1:8000/analyze/<submission_id>?tenantId=tenant-demo"
```

Fetch results:

```bash
curl "http://127.0.0.1:8000/results/<run_id>?tenantId=tenant-demo"
```

## Tests

```bash
pytest
```

## Cosmos DB Design

See:

- `docs/cosmos-schema.md`
- `cosmos/schema-examples.json`

Recommended containers:

- `submissions`
- `threatModelRuns`

Recommended partition key:

- `/tenantId`
