from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    app_name: str = "AI Threat Modeler MVP"
    app_env: str = "development"
    host: str = "127.0.0.1"
    port: int = 8000

    use_mock_analyzer: bool = True
    use_in_memory_store: bool = True

    cosmos_endpoint: str | None = None
    cosmos_key: str | None = None
    cosmos_database: str = "ThreatModeler"
    cosmos_submissions_container: str = "submissions"
    cosmos_runs_container: str = "threatModelRuns"

    azure_openai_endpoint: str | None = None
    azure_openai_api_key: str | None = None
    azure_openai_api_version: str = "2024-10-21"
    azure_openai_deployment: str = "gpt-4o-mini"

settings = Settings()
