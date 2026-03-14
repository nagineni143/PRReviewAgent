using System.Text.Json.Serialization;

public class PullRequestReviewRequest
{
    [JsonPropertyName("event")]
    public string Event { get; set; }
    [JsonPropertyName("body")]
    public string Body { get; set; }
}