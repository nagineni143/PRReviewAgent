using System.Text.Json.Serialization;

public class ReviewIssue
{
    [JsonPropertyName("rule")]
    public string Rule { get; set; }
    public string File { get; set; }
    public int Line { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
}