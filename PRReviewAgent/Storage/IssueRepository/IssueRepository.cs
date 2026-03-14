using System.Collections.Concurrent;

public class IssueRepository
{
    private readonly ConcurrentDictionary<int, List<StoredIssue>> _issues = new();

    public List<StoredIssue> GetIssues(int prNumber)
    {
        return _issues.GetValueOrDefault(prNumber) ?? new List<StoredIssue>();
    }

    public void SaveIssues(int prNumber, List<ReviewIssue> newIssues, List<CodeChange> changes)
    {
        if (!_issues.ContainsKey(prNumber))
            _issues[prNumber] = new List<StoredIssue>();

        foreach (var issue in newIssues)
        {
            var exists = _issues[prNumber]
                .Any(i =>
                    i.Rule == issue.Rule &&
                    i.File == issue.File &&
                    i.Line == issue.Line);

            if (exists)
                continue;

            var change = changes.FirstOrDefault(c =>
                c.File == issue.File &&
                c.LineNumber == issue.Line);

            _issues[prNumber].Add(new StoredIssue
            {
                Id = Guid.NewGuid().ToString(),
                PullRequestNumber = prNumber,
                File = issue.File,
                Line = issue.Line,
                Rule = issue.Rule,
                Description = issue.Description,
                CodeSnippet = change?.Code ?? "",
                Signature = $"{issue.Rule}:{change?.Code}",
                Resolved = false,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    public bool IssueExists(int prNumber, string file, int line, string rule)
    {
        if (!_issues.ContainsKey(prNumber))
            return false;

        return _issues[prNumber]
            .Any(i =>
                i.File == file &&
                i.Line == line &&
                i.Rule == rule &&
                !i.Resolved);
    }

    public void ResolveMissingIssues(int prNumber, List<ReviewIssue> currentIssues)
    {
        if (!_issues.ContainsKey(prNumber))
            return;

        var currentSignatures = currentIssues
            .Select(i => $"{i.Rule}:{i.File}:{i.Line}")
            .ToHashSet();

        foreach (var stored in _issues[prNumber])
        {
            var signature = $"{stored.Rule}:{stored.File}:{stored.Line}";

            if (!currentSignatures.Contains(signature))
            {
                stored.Resolved = true;
            }
        }
    }
}