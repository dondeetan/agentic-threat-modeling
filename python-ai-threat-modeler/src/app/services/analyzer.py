from __future__ import annotations

import json
from typing import Callable, Protocol

from src.app.core.config import settings
from src.app.domain.models import Submission
from src.app.services.prompts import FilePromptContextProvider, PromptBuilder


class AnalyzerTypes:
    # Replace Magic String: central constants keep configured strategy names consistent across the solution.
    openai = "openai"
    azure_openai = "azure-openai"
    mock = "mock"


class AnalyzerProtocol(Protocol):
    analyzer_type: str

    def analyze(self, submission: Submission) -> dict: ...


class MockAnalyzer:
    analyzer_type = AnalyzerTypes.mock

    def analyze(self, submission: Submission) -> dict:
        # Strategy pattern: deterministic analysis can replace AI-backed analysis for tests and local demos.
        components = submission.components
        return {
            "summary": f"{submission.applicationName} should prioritize identity, trust boundary, and data protection controls.",
            "assets": [
                {"name": c, "type": "component", "sensitivity": "medium"} for c in components
            ],
            "trustBoundaries": submission.trustBoundaries,
            "threats": [
                {
                    "id": "TM-001",
                    "strideCategory": "Spoofing",
                    "component": components[0] if components else "Unknown",
                    "threatStatement": "Identity tokens could be spoofed or replayed if validation is weak.",
                    "mitigations": ["Entra ID", "Validate issuer/audience", "Short token lifetimes"],
                },
                {
                    "id": "TM-002",
                    "strideCategory": "Tampering",
                    "component": components[-1] if components else "Unknown",
                    "threatStatement": "Data could be modified through over-privileged access or weak service trust.",
                    "mitigations": ["Managed Identity", "Least privilege RBAC", "Private Endpoints"],
                },
                {
                    "id": "TM-003",
                    "strideCategory": "Information Disclosure",
                    "component": components[-1] if components else "Unknown",
                    "threatStatement": "Sensitive data may leak through logs, queries, or storage misconfiguration.",
                    "mitigations": ["Encryption", "Log scrubbing", "Data classification"],
                },
            ],
            "topPriorities": [
                "Enforce managed identity and least privilege.",
                "Use private connectivity for data services.",
                "Protect logs from sensitive data leakage.",
            ],
        }


class ChatClientProtocol(Protocol):
    def get_response(self, prompt: str) -> str: ...


class ChatClientAnalyzer:
    def __init__(
        self,
        analyzer_type: str,
        chat_client_factory: Callable[[], ChatClientProtocol],
        prompt_context_provider: FilePromptContextProvider | None = None,
        prompt_builder: PromptBuilder | None = None,
    ) -> None:
        self.analyzer_type = analyzer_type
        self._chat_client_factory = chat_client_factory
        self._chat_client: ChatClientProtocol | None = None
        self._prompt_context_provider = prompt_context_provider or FilePromptContextProvider()
        self._prompt_builder = prompt_builder or PromptBuilder()

    @property
    def chat_client(self) -> ChatClientProtocol:
        # Lazy initialization defers external client creation until this strategy is actually selected.
        if self._chat_client is None:
            self._chat_client = self._chat_client_factory()
        return self._chat_client

    def analyze(self, submission: Submission) -> dict:
        # RAG-style context provider: analyzer asks for prompt context without knowing how it is retrieved.
        prompt_context = self._prompt_context_provider.get_context(submission)

        # Strategy pattern: this implementation delegates analysis to a configured chat client.
        response_text = self.chat_client.get_response(self._prompt_builder.build(submission, prompt_context))

        parsed = self._try_read_json(response_text)
        if parsed is not None:
            return parsed

        return {
            "summary": f"{self.analyzer_type} returned an unstructured threat model response.",
            "analyzer": self.analyzer_type,
            "result": response_text,
        }

    @staticmethod
    def _try_read_json(text: str) -> dict | list | None:
        # Adapter-style normalization: structured model output is returned as JSON, otherwise wrapped consistently.
        try:
            return json.loads(text)
        except json.JSONDecodeError:
            return None


class AnalyzerFactory:
    def __init__(self, analyzers: list[AnalyzerProtocol]) -> None:
        self._analyzers = {analyzer.analyzer_type.lower(): analyzer for analyzer in analyzers}

    def create(self, analyzer_type: str | None) -> AnalyzerProtocol:
        # Factory Method: callers ask for an analyzer by capability while construction stays in one place.
        requested_type = analyzer_type.strip() if analyzer_type and analyzer_type.strip() else AnalyzerTypes.openai
        analyzer = self._analyzers.get(requested_type.lower())
        if analyzer:
            return analyzer
        raise ValueError(f"Analyzer '{requested_type}' is not registered.")


_analyzer: AnalyzerProtocol | None = None
_analyzer_factory: AnalyzerFactory | None = None


def get_analyzer() -> AnalyzerProtocol:
    global _analyzer
    if _analyzer is None:
        _analyzer = get_analyzer_factory().create(AnalyzerTypes.mock if settings.use_mock_analyzer else settings.analyzer_type)
    return _analyzer


def get_analyzer_factory() -> AnalyzerFactory:
    global _analyzer_factory
    if _analyzer_factory is None:
        # Dependency Inversion Principle: the API depends on analyzer abstractions, not concrete implementations.
        def create_openai_chat_client() -> ChatClientProtocol:
            from src.app.integrations.openai_chat import OpenAIChatClient

            return OpenAIChatClient()

        def create_azure_openai_chat_client() -> ChatClientProtocol:
            from src.app.integrations.azure_openai import AzureOpenAIChatClient

            return AzureOpenAIChatClient()

        _analyzer_factory = AnalyzerFactory(
            [
                MockAnalyzer(),
                ChatClientAnalyzer(AnalyzerTypes.openai, create_openai_chat_client),
                ChatClientAnalyzer(AnalyzerTypes.azure_openai, create_azure_openai_chat_client),
            ]
        )
    return _analyzer_factory
