public class StoredIssue
{
    public string Id { get; set; }
    public int PullRequestNumber { get; set; }
    public string File { get; set; }
    public int Line { get; set; }
    public string Rule { get; set; }
    public string Description { get; set; }
    public string CodeSnippet { get; set; }
    public string Signature { get; set; }
    public bool Resolved { get; set; }
    public DateTime CreatedAt { get; set; }
}