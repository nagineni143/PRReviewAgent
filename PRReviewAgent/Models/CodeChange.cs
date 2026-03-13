public class CodeChange
{
    public string File { get; set; }
    public int LineNumber { get; set; }
    public string Code { get; set; }
    public List<string> RuleHints { get; set; } = new();
}