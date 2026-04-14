from fastapi.testclient import TestClient
from src.app.main import app

client = TestClient(app)

def test_submit_analyze_results_flow():
    payload = {
        "tenantId": "tenant-demo",
        "applicationName": "Claims API",
        "businessPurpose": "Processes insurance claims",
        "architectureSummary": "React SPA -> API Management -> App Service -> Azure SQL",
        "components": ["React SPA", "API Management", "App Service", "Azure SQL"],
        "dataFlows": ["Browser to SPA", "SPA to API Management", "App Service to Azure SQL"],
        "trustBoundaries": ["Internet to Azure Edge", "Application Tier to Data Tier"],
        "authenticationDetails": "Entra ID and Managed Identity",
        "sensitiveData": ["PII"],
        "internetExposure": "Public SPA and API",
        "existingControls": ["WAF"],
        "assumptions": ["No public SQL access"],
    }
    submit_res = client.post("/submit", json=payload)
    assert submit_res.status_code == 200
    submission = submit_res.json()

    analyze_res = client.post(f"/analyze/{submission['id']}?tenantId=tenant-demo")
    assert analyze_res.status_code == 200
    run = analyze_res.json()

    result_res = client.get(f"/results/{run['runId']}?tenantId=tenant-demo")
    assert result_res.status_code == 200
    body = result_res.json()
    assert body["result"]["threats"]
