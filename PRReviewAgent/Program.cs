using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<OpenAIClientFactory>();
builder.Services.AddHttpClient<GitHubPRClient>();
builder.Services.AddScoped<ChangeBatcher>();
builder.Services.AddScoped<DiffService>();
builder.Services.AddScoped<ReviewAgent>();
builder.Services.AddSingleton<IssueRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGet("/pr/{number}", async (int number, GitHubPRClient client) =>
{
    var files = await client.GetPullRequestFiles(number);

    return files;
})
.WithName("GetPRDetails");
app.MapGet("/pr/{number}/changes", async (
    int number,
    GitHubPRClient github,
    DiffService diff) =>
{
    var files = await github.GetPullRequestFiles(number);

    var changes = diff.ExtractChanges(files);

    return changes;
})
.WithName("GetCleanCodeChangesForReview");

app.MapGet("/pr/{number}/review", async (
    int number,
    GitHubPRClient github,
    DiffService diff,
    ReviewAgent agent,
    IssueRepository repo,
    ChangeBatcher batcher) =>
{
    var files = await github.GetPullRequestFiles(number);

    var changes = diff.ExtractChanges(files);
    var batchChanges = batcher.CreateBatches(changes);

    var review = await agent.ReviewBatchAsync(batchChanges);
    repo.ResolveMissingIssues(number, review.Issues);

    var commit = await github.GetLatestCommit(number);

    foreach (var issue in review.Issues)
    {
        if (repo.IssueExists(number, issue.File, issue.Line, issue.Rule))
            continue;

        var message = $"❌ {issue.Rule} violation\n{issue.Description}";

        await github.AddLineComment(
            number,
            commit,
            issue.File,
            issue.Line,
            message);
    }
    repo.SaveIssues(number, review.Issues, changes);

    if (!repo.GetIssues(number).Any(i => !i.Resolved))
    {
        await github.DismissPendingReviews(number);
        await github.ApprovePullRequest(number);
    }

    return review;
})
.WithName("LLMReview");

app.Run();
