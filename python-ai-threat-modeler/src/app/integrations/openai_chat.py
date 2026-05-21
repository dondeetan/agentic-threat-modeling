from __future__ import annotations

from openai import OpenAI

from src.app.core.config import settings


class OpenAIChatClient:
    def __init__(self) -> None:
        if not settings.openai_api_key:
            raise ValueError("OpenAI configuration is missing.")
        self._client = OpenAI(api_key=settings.openai_api_key, base_url=settings.openai_endpoint)

    def get_response(self, prompt: str) -> str:
        response = self._client.chat.completions.create(
            model=settings.openai_model,
            messages=[
                {"role": "system", "content": "Return valid JSON only."},
                {"role": "user", "content": prompt},
            ],
            temperature=0.2,
        )
        return response.choices[0].message.content or "{}"
