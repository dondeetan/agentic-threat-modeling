def build_threat_model_prompt(payload: dict) -> str:
    return f'''
You are a senior cloud security threat modeling assistant.
Analyze the system with STRIDE.
Focus on Azure and Microsoft-native controls when relevant.
Return JSON only with keys:
summary, assets, trustBoundaries, threats, topPriorities.

Input:
{payload}
'''.strip()
