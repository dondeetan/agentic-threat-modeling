from src.app.domain.models import Submission
from src.app.services.analyzer import AnalyzerFactory, ChatClientAnalyzer, MockAnalyzer


class StubChatClient:
    def __init__(self, response: str) -> None:
        self.response = response

    def get_response(self, prompt: str) -> str:
        return self.response


def create_submission() -> Submission:
    return Submission(
        tenantId="tenant-a",
        applicationName="Payments API",
        businessPurpose="Process card payments",
        architectureSummary="Gateway to processor",
        components=["Gateway", "Processor"],
        dataFlows=["Gateway to Processor"],
        trustBoundaries=["Internet", "App to Data"],
        openApiDocument="{}",
        authenticationDetails="Entra ID",
        sensitiveData=["PII"],
        internetExposure="Public API",
        existingControls=["WAF"],
        assumptions=["No public DB access"],
    )


def test_mock_analyzer_returns_expected_threat_structure():
    result = MockAnalyzer().analyze(create_submission())

    assert result["summary"] == "Payments API should prioritize identity, trust boundary, and data protection controls."
    assert len(result["assets"]) == 2
    assert len(result["threats"]) == 3
    assert result["threats"][0]["component"] == "Gateway"
    assert result["threats"][1]["component"] == "Processor"


def test_analyzer_factory_uses_openai_by_default():
    analyzer = ChatClientAnalyzer("openai", lambda: StubChatClient('{"summary":"ok"}'))
    factory = AnalyzerFactory([analyzer])

    assert factory.create("").analyzer_type == "openai"


def test_chat_client_analyzer_returns_json_when_model_output_is_structured():
    analyzer = ChatClientAnalyzer("openai", lambda: StubChatClient('{"summary":"ok"}'))

    result = analyzer.analyze(create_submission())

    assert result == {"summary": "ok"}


def test_chat_client_analyzer_wraps_unstructured_model_output():
    analyzer = ChatClientAnalyzer("openai", lambda: StubChatClient("plain text threat model"))

    result = analyzer.analyze(create_submission())

    assert result["summary"] == "openai returned an unstructured threat model response."
    assert result["analyzer"] == "openai"
    assert result["result"] == "plain text threat model"
