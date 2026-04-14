# .NET AI Threat Modeler MVP

A runnable ASP.NET Core minimal API MVP for automated security threat modeling in an Azure/Microsoft environment.

## What this repo includes

- ASP.NET Core API with endpoints:
  - `POST /submit`
  - `POST /analyze/{submissionId}`
  - `GET /results/{runId}`
  - `GET /health`
- Cosmos DB repository with **in-memory fallback**
- Mock analyzer with deterministic STRIDE output
- Azure OpenAI adapter scaffold
- Clean-ish project separation for quick MVP work

## Repository structure

```text
dotnet-ai-threat-modeler-mvp/
├─ README.md
├─ .env.example
├─ AiThreatModeler.sln
├─ src/
│  ├─ Api/
│  │  ├─ Program.cs
│  │  ├─ appsettings.json
│  │  ├─ appsettings.Development.json
│  │  └─ Api.csproj
│  └─ ThreatModeler/
│     ├─ ThreatModeler.csproj
│     ├─ Models/
│     │  ├─ Submission.cs
│     │  ├─ ThreatModelRun.cs
│     │  └─ Dtos.cs
│     ├─ Services/
│     │  ├─ IAnalyzer.cs
│     │  ├─ MockAnalyzer.cs
│     │  ├─ AzureOpenAiAnalyzer.cs
│     │  ├─ ISubmissionStore.cs
│     │  ├─ InMemorySubmissionStore.cs
│     │  └─ CosmosSubmissionStore.cs
│     └─ Configuration/
│        └─ AppOptions.cs
├─ tests/
│  └─ ThreatModeler.Tests/
├─ docs/
│  └─ cosmos-schema.md
└─ cosmos/
   └─ schema-examples.json
```

## Local development prerequisites

- .NET 10 SDK
- Optional:
  - Azure Cosmos DB account
  - Azure OpenAI deployment

## Quick start

### 1) Restore packages

```bash
dotnet restore
```

### 2) Configure settings

Copy `.env.example` values into `src/Api/appsettings.Development.json` or environment variables.

Recommended local defaults:
- `UseMockAnalyzer=true`
- `UseInMemoryStore=true`

### 3) Run locally

```bash
dotnet run --project src/Api
```

The API will start on:
- `http://localhost:5099` or the port assigned by ASP.NET Core

### 4) Swagger

Open:
- `/swagger`

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

## Notes

- This repo is structured to run locally even before Azure resources exist.
- The in-memory path is useful for developer onboarding and demos.
- When Azure services are ready, switch off mock/in-memory mode and populate the corresponding settings.
