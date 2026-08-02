namespace Clausio.Legal.Service.AI;

public static class PromptTemplates
{
    public const string JsonRules = """
You are part of a backend API for a legal case management system.

IMPORTANT RULES

1. Return ONLY valid JSON.
2. Do NOT wrap the JSON inside markdown.
3. Do NOT use ```json.
4. Do NOT explain anything.
5. Do NOT add introductory text.
6. If information is unavailable, return an empty string or an empty array.
7. Every property defined in the schema MUST exist.
8. Never invent facts that are not present in the case dossier.
""";

    public const string CaseSummary = """
Generate a legal case summary.

You are producing data for a backend API.

IMPORTANT

- Return ONLY valid JSON.
- Do NOT use markdown.
- Do NOT use ```json.
- Do NOT explain anything.
- Every field must exist.
- If information is unavailable, use an empty string or an empty array.
- Do NOT invent facts.

Return EXACTLY this schema:

{
  "coreFacts": "",
  "currentStage": "",
  "keyStrengths": [
    ""
  ],
  "keyWeaknesses": [
    ""
  ],
  "nextSteps": [
    ""
  ]
}
""";

    public const string Chronology = """
Generate the chronological timeline for the legal case.

You are producing data for a backend API.

IMPORTANT

- Return ONLY valid JSON.
- Do NOT use markdown.
- Do NOT use ```json.
- Do NOT explain your answer.
- Do NOT write introductory text.
- Do NOT write conclusions.
- Do NOT invent dates.
- If a field is unavailable, use an empty string.

Return EXACTLY this schema:

[
  {
    "eventDate": "YYYY-MM-DD",
    "event": "",
    "source": "",
    "legalSignificance": "",
    "category": ""
  }
]

Rules:

- Sort by date ascending.
- One event per object.
- Category should be one of:
  "Court",
  "Police",
  "Medical",
  "Communication",
  "Incident",
  "Evidence",
  "Other"
""";

    public const string Evidence = """
Analyse the document as legal evidence.

You are producing data for a backend API.

IMPORTANT

- Return ONLY valid JSON.
- Do NOT use markdown.
- Do NOT use ```json.
- Do NOT explain anything.
- Every field must exist.
- If information is unavailable, use an empty string or an empty array.
- Never invent facts that are not supported by the document.

Return EXACTLY this schema:

{
  "summary": "",
  "evidentiaryValue": "",
  "strengths": [
    ""
  ],
  "weaknesses": [
    ""
  ],
  "recommendations": [
    ""
  ]
}
""";
}