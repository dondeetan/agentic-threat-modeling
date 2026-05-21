from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path

from src.app.domain.models import Submission


@dataclass(frozen=True)
class PromptContext:
    # Value object: retrieved prompt fragments move through the analyzer as one immutable unit.
    systemRules: str
    guidelines: str
    outputFormat: str


class FilePromptContextProvider:
    system_rules_file_name = "system-rules.md"
    output_format_file_name = "output-format.json"

    def __init__(self, prompt_root_directory: str | Path | None = None) -> None:
        # Dependency Inversion Principle: callers depend on the provider contract, not this file-system detail.
        self.prompt_root_directory = Path(prompt_root_directory) if prompt_root_directory else Path(__file__).resolve().parents[1] / "prompts"

    def get_context(self, submission: Submission) -> PromptContext:
        # Template-style flow: required prompt fragments are always loaded, optional guidance is selected by submission context.
        system_rules = self._read_required_prompt_file(self.system_rules_file_name)
        output_format = self._read_required_prompt_file(self.output_format_file_name)
        guideline_file_names = self._select_guideline_file_names(submission)

        guidelines: list[str] = []
        for guideline_file_name in guideline_file_names:
            content = self._read_required_prompt_file(guideline_file_name)
            guidelines.append(f"## {Path(guideline_file_name).stem}")
            guidelines.append(content.strip())
            guidelines.append("")

        return PromptContext(system_rules.strip(), "\n".join(guidelines).strip(), output_format.strip())

    @classmethod
    def _select_guideline_file_names(cls, submission: Submission) -> list[str]:
        # Open/Closed Principle: new guideline files can be added here without changing analyzer transport logic.
        guideline_file_names = [
            "threat-modeling-guidelines.md",
            "stride.md",
        ]

        if submission.internetExposure.strip() or submission.trustBoundaries or submission.components:
            guideline_file_names.append("cloud-controls.md")

        if submission.authenticationDetails.strip() or cls._has_any_value_containing(
            submission.components, "identity", "auth", "login", "token"
        ):
            guideline_file_names.append("identity-controls.md")

        if submission.sensitiveData or cls._has_any_value_containing(
            submission.dataFlows, "pii", "payment", "secret", "credential"
        ):
            guideline_file_names.append("data-protection.md")

        return guideline_file_names

    def _read_required_prompt_file(self, file_name: str) -> str:
        path = self.prompt_root_directory / file_name
        if not path.exists():
            raise FileNotFoundError(f"Prompt context file '{file_name}' was not found: {path}")
        return path.read_text(encoding="utf-8")

    @staticmethod
    def _has_any_value_containing(values: list[str], *needles: str) -> bool:
        return any(needle.lower() in value.lower() for value in values for needle in needles)


class PromptBuilder:
    def build(self, submission: Submission, context: PromptContext) -> str:
        # Single Responsibility Principle: this class only composes the final model prompt.
        payload = json.dumps(submission.to_prompt_dict(), indent=2)

        return f"""# System Rules
{context.systemRules}

# Retrieved Guidelines
{context.guidelines}

# Output Format
{context.outputFormat}

# Submission
{payload}"""


def build_threat_model_prompt(payload: dict) -> str:
    submission = Submission.from_dict(payload)
    return PromptBuilder().build(submission, FilePromptContextProvider().get_context(submission))
