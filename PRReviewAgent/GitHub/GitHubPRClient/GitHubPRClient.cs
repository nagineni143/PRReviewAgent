using System.Net.Http.Headers;
using System.Text.Json;

public class GitHubPRClient
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public GitHubPRClient(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;

        var token = _config["GitHub:Token"];

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        _http.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("PRReviewAgent", "1.0"));
    }

    public async Task<List<PullRequestFile>> GetPullRequestFiles(int pullNumber)
    {
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repository"];

        var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{pullNumber}/files";

        var response = await _http.GetFromJsonAsync<List<PullRequestFile>>(url);

        return response ?? new List<PullRequestFile>();
    }

    public async Task AddLineComment(
    int prNumber,
    string commitId,
    string file,
    int line,
    string message)
    {
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repository"];

        var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}/comments";

        var payload = new
        {
            body = message,
            commit_id = commitId,
            path = file,
            line = line
        };

        Console.WriteLine("payload....", payload.body);

        await _http.PostAsJsonAsync(url, payload);
    }

    public async Task<string> GetLatestCommit(int prNumber)
    {
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repository"];

        var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}";

        var pr = await _http.GetFromJsonAsync<JsonElement>(url);

        return pr.GetProperty("head").GetProperty("sha").GetString();
    }

    public async Task ApprovePullRequest(int prNumber)
    {
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repository"];

        var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}/reviews";

        var payload = new PullRequestReviewRequest
        {
            Event = "APPROVE",
            Body = "✅ AI reviewer: All coding standards satisfied."
        };

        var response = await _http.PostAsJsonAsync(url, payload);

        var text = await response.Content.ReadAsStringAsync();

        Console.WriteLine(text);
    }

    public async Task DismissPendingReviews(int prNumber)
    {
        var owner = _config["GitHub:Owner"];
        var repo = _config["GitHub:Repository"];

        var url = $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}/reviews";

        var reviews = await _http.GetFromJsonAsync<List<JsonElement>>(url);

        foreach (var review in reviews)
        {
            var state = review.GetProperty("state").GetString();

            if (state == "PENDING" || state == "COMMENTED")
            {
                var id = review.GetProperty("id").GetInt64();

                var dismissUrl =
                    $"https://api.github.com/repos/{owner}/{repo}/pulls/{prNumber}/reviews/{id}/dismissals";

                await _http.PutAsJsonAsync(dismissUrl, new
                {
                    message = "Superseded by new review"
                });
            }
        }
    }
}