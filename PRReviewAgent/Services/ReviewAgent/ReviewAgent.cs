using System.Text.Json;
using OpenAI.Chat;

public class ReviewAgent
{
    private readonly ChatClient _chatClient;
    private readonly string _codingStandards;

    public ReviewAgent(OpenAIClientFactory infra)
    {
        _chatClient = infra.GetChatClient();

        _codingStandards = File.ReadAllText("docs/coding_standards.md");
    }

    public async Task<ReviewResult> ReviewBatchAsync(List<ReviewBatch> reviewBatches)
    {
        var result = new ReviewResult();

        foreach (var reviewBatch in reviewBatches)
        {
            foreach (var change in reviewBatch.Changes)
            {
                var hints = change.RuleHints.Count > 0
                        ? $"Possible rule violations: {string.Join(", ", change.RuleHints)}"
                        : "No obvious rule hints detected.";
                var prompt = $@"
            You are an automated code reviewer.

            {hints}

            You must review the code strictly using the coding standards below.

            If the code violates a rule, return JSON:

            {{
                ""rule"": ""RULE_ID"",
                ""description"": ""short explanation""
            }}

            Return JSON only.

            If no issues exist return:

            {{
                ""rule"": ""NONE""
            }}

            Coding Standards:
            {_codingStandards}

            File: {change.File}
            Line: {change.LineNumber}

            Code:
            {change.Code}
            ";

                var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a strict code review agent."),
                new UserChatMessage(prompt)
            };

                var response = await _chatClient.CompleteChatAsync(messages);

                var content = response.Value.Content[0].Text.Trim();

                if (content.Contains("OK", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var issue = JsonSerializer.Deserialize<ReviewIssue>(content);

                    if (issue == null || string.IsNullOrWhiteSpace(issue.Rule))
                        continue;

                    issue.File = change.File;
                    issue.Line = change.LineNumber;

                    result.Issues.Add(issue);
                }
                catch
                {
                    // ignore malformed response
                }
            }
        }

        return result;
    }
}