from pathlib import Path
import shutil
import uuid

from src.app.domain.models import Submission
from src.app.services.prompts import FilePromptContextProvider, PromptBuilder


def create_prompt_root(tmp_path: Path) -> Path:
    (tmp_path / "system-rules.md").write_text("system rules", encoding="utf-8")
    (tmp_path / "output-format.json").write_text('{ "summary": "string" }', encoding="utf-8")
    (tmp_path / "threat-modeling-guidelines.md").write_text("baseline content", encoding="utf-8")
    (tmp_path / "stride.md").write_text("stride content", encoding="utf-8")
    (tmp_path / "cloud-controls.md").write_text("cloud content", encoding="utf-8")
    (tmp_path / "identity-controls.md").write_text("identity content", encoding="utf-8")
    (tmp_path / "data-protection.md").write_text("data content", encoding="utf-8")
    return tmp_path


def create_workspace_prompt_root() -> Path:
    root = Path(__file__).resolve().parent / ".tmp" / f"prompts-{uuid.uuid4().hex}"
    root.mkdir(parents=True)
    return create_prompt_root(root)


def create_submission(**overrides) -> Submission:
    values = {
        "tenantId": "tenant-a",
        "applicationName": "Payments API",
        "businessPurpose": "Process card payments",
        "architectureSummary": "Gateway to processor",
        "components": [],
        "dataFlows": [],
        "trustBoundaries": [],
        "openApiDocument": "{}",
        "authenticationDetails": "",
        "sensitiveData": [],
        "internetExposure": "",
        "existingControls": [],
        "assumptions": [],
    }
    values.update(overrides)
    return Submission(**values)


def test_file_prompt_context_provider_loads_required_and_relevant_guidelines():
    prompt_root = create_workspace_prompt_root()
    try:
        provider = FilePromptContextProvider(prompt_root)
        submission = create_submission(
            components=["Identity API"],
            internetExposure="Public",
            sensitiveData=["PII"],
        )

        context = provider.get_context(submission)

        assert context.systemRules == "system rules"
        assert context.outputFormat == '{ "summary": "string" }'
        assert "threat-modeling-guidelines" in context.guidelines
        assert "stride content" in context.guidelines
        assert "cloud content" in context.guidelines
        assert "identity content" in context.guidelines
        assert "data content" in context.guidelines
    finally:
        shutil.rmtree(prompt_root.parent, ignore_errors=True)


def test_file_prompt_context_provider_uses_baseline_guidelines_for_minimal_submissions():
    prompt_root = create_workspace_prompt_root()
    try:
        provider = FilePromptContextProvider(prompt_root)

        context = provider.get_context(create_submission())

        assert "threat-modeling-guidelines" in context.guidelines
        assert "stride content" in context.guidelines
        assert "cloud content" not in context.guidelines
        assert "identity content" not in context.guidelines
        assert "data content" not in context.guidelines
    finally:
        shutil.rmtree(prompt_root.parent, ignore_errors=True)


def test_prompt_builder_composes_retrieved_context_and_submission_payload():
    prompt = PromptBuilder().build(
        create_submission(applicationName="Payments API", businessPurpose="Process card payments", components=["Gateway"]),
        context=type(
            "Context",
            (),
            {
                "systemRules": "System rule text",
                "guidelines": "Guideline text",
                "outputFormat": '{ "summary": "string" }',
            },
        )(),
    )

    assert "# System Rules" in prompt
    assert "System rule text" in prompt
    assert "# Retrieved Guidelines" in prompt
    assert "Guideline text" in prompt
    assert "# Output Format" in prompt
    assert '{ "summary": "string" }' in prompt
    assert '"ApplicationName": "Payments API"' in prompt
