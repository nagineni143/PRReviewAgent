1. GitHubPRClient - connect with github for PR details
Response:
[
  {
    "filename": "UserService.cs",
    "additions": 5,
    "deletions": 1,
    "patch": "@@ -10,7 +10,7 @@\n- var result = service.GetDataAsync().Result;\n+ var result = await service.GetDataAsync();"
  }
]

2. DiffService - Process above result and returns only newly added changes
Response:
[
  {
    "file": "Chat/ChatGPT/Infrastructure/OpenAIInfrastructure.cs",
    "lineNumber": 9,
    "code": "var apiKey = Environment.GetEnvironmentVariable(\"OPENAI_API_KEY\");"
  }
]

3. ChangeBatcher - From above result it groups changes by file, 20 changes per batch to reduce llm calls, rate-limit issues

4. ReviewAgent - Get details from above and submit it to LLM for review.
Response:
If Code violates rule:
{
  "issues": [
    {
      "rule": "ASYNC002",
      "file": "UserService.cs",
      "line": 42,
      "description": "Blocking async call using .Result"
    }
  ],
  "approved": false
}
If no issues:
{
  "issues": [],
  "approved": true
}