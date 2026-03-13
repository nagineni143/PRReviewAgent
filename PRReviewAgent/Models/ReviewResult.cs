public class ReviewResult
{
    public List<ReviewIssue> Issues { get; set; } = new();
    public bool Approved => Issues.Count == 0;
}