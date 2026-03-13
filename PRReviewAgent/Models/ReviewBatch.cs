public class ReviewBatch
{
    public string File { get; set; }
    public List<CodeChange> Changes { get; set; } = new();
}