from __future__ import annotations
import json
from openai import AzureOpenAI
from src.app.core.config import settings
from src.app.services.prompts import build_threat_model_prompt

class AzureOpenAIAnalyzer:
    analyzer_type = "azure-openai"

    def __init__(self) -> None:
        if not settings.azure_openai_endpoint or not settings.azure_openai_api_key:
            raise ValueError("Azure OpenAI configuration is missing.")
        self.client = AzureOpenAI(
            api_key=settings.azure_openai_api_key,
            api_version=settings.azure_openai_api_version,
            azure_endpoint=settings.azure_openai_endpoint,
        )

    def analyze(self, submission: dict) -> dict:
        prompt = build_threat_model_prompt(submission)
        response = self.client.chat.completions.create(
            model=settings.azure_openai_deployment,
            messages=[
                {"role": "system", "content": "Return valid JSON only."},
                {"role": "user", "content": prompt},
            ],
            temperature=0.2,
        )
        content = response.choices[0].message.content or "{}"
        try:
            return json.loads(content)
        except json.JSONDecodeError:
            return {
                "summary": "Model output was not valid JSON.",
                "assets": [],
                "trustBoundaries": submission.get("trustBoundaries", []),
                "threats": [],
                "topPriorities": ["Review Azure OpenAI prompt/output handling."],
                "rawOutput": content,
            }
