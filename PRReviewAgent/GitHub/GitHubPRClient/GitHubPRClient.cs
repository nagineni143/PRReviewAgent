using System.Net.Http.Headers;

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
}