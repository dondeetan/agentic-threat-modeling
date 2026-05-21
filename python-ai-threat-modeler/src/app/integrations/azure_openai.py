from __future__ import annotations

from openai import AzureOpenAI

from src.app.core.config import settings


class AzureOpenAIChatClient:
    def __init__(self) -> None:
        if not settings.azure_openai_endpoint or not settings.azure_openai_api_key:
            raise ValueError("Azure OpenAI configuration is missing.")
        self._client = AzureOpenAI(
            api_key=settings.azure_openai_api_key,
            api_version=settings.azure_openai_api_version,
            azure_endpoint=settings.azure_openai_endpoint,
        )

    def get_response(self, prompt: str) -> str:
        response = self._client.chat.completions.create(
            model=settings.azure_openai_deployment,
            messages=[
                {"role": "system", "content": "Return valid JSON only."},
                {"role": "user", "content": prompt},
            ],
            temperature=0.2,
        )
        return response.choices[0].message.content or "{}"


AzureOpenAIAnalyzer = AzureOpenAIChatClient
