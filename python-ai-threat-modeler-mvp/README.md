# Python AI Threat Modeler MVP

A runnable FastAPI MVP for automated security threat modeling in an Azure/Microsoft environment.

## What this repo includes

- FastAPI API with endpoints:
  - `POST /submit`
  - `POST /analyze/{submission_id}`
  - `GET /results/{run_id}`
  - `GET /health`
- Cosmos DB support with **local in-memory fallback**
- Azure OpenAI client support with **mock analyzer fallback**
- Basic STRIDE-based threat model generation
- Blob/Cosmos/Azure OpenAI configuration via environment variables
- Tests for the main flow

## Repository structure

```text
python-ai-threat-modeler-mvp/
├─ README.md
├─ .env.example
├─ requirements.txt
├─ pyproject.toml
├─ src/
│  └─ app/
│     ├─ main.py
│     ├─ api/routes.py
│     ├─ core/
│     │  ├─ config.py
│     │  └─ logging.py
│     ├─ domain/
│     │  ├─ models.py
│     │  └─ schemas.py
│     ├─ services/
│     │  ├─ analyzer.py
│     │  ├─ prompts.py
│     │  └─ storage.py
│     └─ integrations/
│        ├─ azure_openai.py
│        └─ cosmos.py
├─ tests/
│  └─ test_api.py
├─ docs/
│  └─ cosmos-schema.md
└─ cosmos/
   └─ schema-examples.json
```

## Local development prerequisites

- Python 3.11+
- Optional:
  - Azure Cosmos DB account
  - Azure OpenAI deployment

## Quick start

### 1) Create and activate a virtual environment

Windows PowerShell:
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

macOS/Linux:
```bash
python -m venv .venv
source .venv/bin/activate
```

### 2) Install dependencies

```bash
pip install -r requirements.txt
```

### 3) Configure environment

Copy `.env.example` to `.env` and update values as needed.

- Leave `USE_MOCK_ANALYZER=true` to run locally without Azure OpenAI.
- Leave `USE_IN_MEMORY_STORE=true` to run locally without Cosmos DB.

### 4) Run locally

```bash
uvicorn src.app.main:app --reload
```

The API will start on `http://127.0.0.1:8000`.

### 5) Open API docs

- Swagger UI: `http://127.0.0.1:8000/docs`

## Example flow

### Submit a workload

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
    "authenticationDetails": "Entra ID for users, Managed Identity for service-to-service",
    "sensitiveData": ["PII", "financial data"],
    "internetExposure": "Public SPA and API entrypoint",
    "existingControls": ["WAF", "Key Vault", "Defender for Cloud"],
    "assumptions": ["No public access to SQL"]
  }'
```

### Analyze the submitted workload

```bash
curl -X POST http://127.0.0.1:8000/analyze/<submission_id>
```

### Fetch the results

```bash
curl http://127.0.0.1:8000/results/<run_id>
```

## Local test run

```bash
pytest
```

## Cosmos DB design

See:
- `docs/cosmos-schema.md`
- `cosmos/schema-examples.json`

Recommended containers:
- `submissions`
- `threatModelRuns`
- `threatFindings`
- `reviews`
- `workItemLinks`

Recommended partition key:
- `/tenantId`

## Notes

- This MVP is designed to be **runnable first**, then connected to Azure services.
- The mock analyzer gives you deterministic results for development and demos.
- When you are ready for Azure OpenAI, set `USE_MOCK_ANALYZER=false` and populate the Azure values in `.env`.
