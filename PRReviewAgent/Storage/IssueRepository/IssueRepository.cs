using System.Collections.Concurrent;

public class IssueRepository
{
    private readonly ConcurrentDictionary<int, List<StoredIssue>> _issues = new();

    public List<StoredIssue> GetIssues(int prNumber)
    {
        return _issues.GetValueOrDefault(prNumber) ?? new List<StoredIssue>();
    }

    public void SaveIssues(int prNumber, List<ReviewIssue> newIssues)
    {
        var stored = newIssues.Select(i => new StoredIssue
        {
            Id = Guid.NewGuid().ToString(),
            PullRequestNumber = prNumber,
            File = i.File,
            Line = i.Line,
            Rule = i.Rule,
            Description = i.Description,
            Resolved = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        _issues[prNumber] = stored;
    }

    public void MarkResolved(int prNumber, string file, int line)
    {
        if (!_issues.ContainsKey(prNumber))
            return;

        var issue = _issues[prNumber]
            .FirstOrDefault(i => i.File == file && i.Line == line);

        if (issue != null)
            issue.Resolved = true;
    }
}