# .NET OpenAPI Threat Modeler MVP

A runnable ASP.NET Core minimal API for automated security threat modeling. The solution now treats OpenAPI as the default analyzer input so API specifications can drive repeatable STRIDE-oriented analysis before Azure OpenAI or other AI services are wired in.

## Solution architecture

![Threat Modeler AI solution architecture](docs/ThreatModelerAIV1.png)

The architecture in `docs/ThreatModelerAIV1.png` shows the long-term platform shape:

- Security architects and DevSecOps teams submit app descriptions, DFD context, infrastructure/repo details, API specs, diagrams, and known controls.
- The Threat Modeling Portal/API receives the request and runs under Azure hosting options such as App Service, AKS, or Functions with Entra ID authentication and managed identity.
- The orchestration layer coordinates specialized capabilities such as STRIDE classification, trust-boundary extraction, MITRE mapping, NIST/CIS control mapping, risk scoring, and ticket writing.
- Azure OpenAI/Foundry, Azure AI Search, and Microsoft Security integrations enrich the analysis with reasoning, retrieval, telemetry, prior threat models, secure patterns, asset inventory, and security findings.
- Outputs include a threat register, STRIDE by component, attack paths, control recommendations, risk scores, and Azure DevOps/GitHub work items.

This repository implements the API and orchestration core of that architecture. The current default path is deterministic OpenAPI analysis, which keeps local development and tests reliable while preserving extension points for Azure OpenAI, AI Search, Cosmos DB, and ticket/security integrations.

## Design approach

The refactor follows the pattern references under `DotNet/Patterns` in `dondeetan/best-practices-poc`:

- **Strategy pattern**: `IAnalyzer` allows `OpenApiAnalyzer`, `MockAnalyzer`, and `AzureOpenAiAnalyzer` to be swapped without changing workflow code.
- **Factory Method pattern**: `AnalyzerFactory` selects the configured analyzer. `openapi` is the default.
- **Facade pattern**: `SubmissionWorkflow` gives the API one application-service entry point for submit, analyze, and result retrieval.
- **Repository pattern**: `ISubmissionStore` hides in-memory and future Cosmos DB persistence.
- **Proxy pattern**: `CosmosSubmissionStore` preserves the Cosmos-facing repository contract until the live SDK implementation is added.
- **SOLID principles**: parsing, workflow orchestration, analyzer selection, persistence, and endpoint composition are separated so each type has a focused reason to change.

The code includes short comments at the implementation points where these patterns or principles are applied.

## What this repo includes

- ASP.NET Core API with endpoints:
  - `GET /openapi/v1.json`
  - `POST /submit`
  - `POST /analyze/{submissionId}`
  - `GET /results/{runId}`
  - `GET /health`
- OpenAPI analyzer with deterministic endpoint extraction and STRIDE-style output
- Analyzer factory for OpenAPI, OpenAI, Azure OpenAI, and mock analyzers
- Cosmos DB repository scaffold with in-memory local store
- Tests for workflow, store behavior, mock analyzer behavior, and OpenAPI analyzer selection

## Repository structure

```text
dotnet-ai-threat-modeler/
+-- README.md
+-- .env.example
+-- AiThreatModeler.sln
+-- src/
|   +-- Api/
|   |   +-- Program.cs
|   |   +-- appsettings.json
|   |   +-- appsettings.Development.json
|   |   \-- Api.csproj
|   +-- ThreatModeler/
|   |   +-- Configuration/
|   |   +-- Models/
|   |   \-- Services/
|   \-- ThreatModeler.Tests/
+-- docs/
|   +-- ThreatModelerAIV1.png
|   \-- cosmos-schema.md
\-- cosmos/
    \-- schema-examples.json
```

## Local development prerequisites

- .NET 10 SDK
- Optional future integrations:
  - Azure Cosmos DB account
  - Azure OpenAI or Azure AI Foundry deployment

## Quick start

### 1. Restore packages

```bash
dotnet restore
```

### 2. Configure settings

Use `src/Api/appsettings.Development.json` or environment variables.

Recommended local defaults:

```json
{
  "App": {
    "AnalyzerType": "openapi",
    "UseInMemoryStore": true
  }
}
```

Supported analyzer types:

- `openapi`
- `openai`
- `azure-openai`
- `mock`

When `AnalyzerType` is `openai`, configure the `OpenAI` section with `Endpoint`, `ApiKey`, and `Model`.
When `AnalyzerType` is `azure-openai`, configure the `AzureOpenAI` section with `Endpoint`, `ApiKey`, and `Model`.

### 3. Run locally

```bash
dotnet run --project src/Api
```

The API starts on the ASP.NET Core assigned local port, commonly `http://localhost:5099`.

### 4. OpenAPI document

Open:

- `/openapi/v1.json`

## Example flow

### Submit

```bash
curl -X POST http://localhost:5099/submit \
  -H "Content-Type: application/json" \
  -d '{
    "tenantId": "tenant-demo",
    "applicationName": "Claims API",
    "businessPurpose": "Processes insurance claims",
    "architectureSummary": "React SPA -> API Management -> App Service -> Azure SQL",
    "components": ["React SPA", "API Management", "App Service", "Azure SQL"],
    "dataFlows": ["Browser to SPA", "SPA to API Management", "App Service to Azure SQL"],
    "trustBoundaries": ["Internet to Azure Edge", "Application Tier to Data Tier"],
    "openApiDocument": "{\"openapi\":\"3.0.1\",\"paths\":{\"/claims\":{\"get\":{},\"post\":{}}}}",
    "authenticationDetails": "Entra ID for users, Managed Identity for service-to-service",
    "sensitiveData": ["PII", "financial data"],
    "internetExposure": "Public SPA and API entrypoint",
    "existingControls": ["WAF", "Key Vault", "Defender for Cloud"],
    "assumptions": ["No public access to SQL"]
  }'
```

### Analyze

```bash
curl -X POST "http://localhost:5099/analyze/<submissionId>?tenantId=tenant-demo"
```

### Results

```bash
curl "http://localhost:5099/results/<runId>?tenantId=tenant-demo"
```

## Integration notes

- OpenAPI is the default analyzer so the MVP can produce stable local output from API specs.
- OpenAI and Azure OpenAI analyzers use the shared `ChatClientShared` factories and can be selected with `App:AnalyzerType`.
- Cosmos DB remains behind `ISubmissionStore`; switch `UseInMemoryStore=false` only after adding the live Cosmos SDK implementation.
