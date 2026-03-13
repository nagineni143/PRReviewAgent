# Coding Standards for AI PR Review Agent

## Purpose

This document defines the coding standards and performance guidelines
used by the automated PR review agent.

The AI reviewer must evaluate pull requests **only against the rules
defined in this document**.

If no violations exist, the pull request must be marked as:

APPROVED

The reviewer must **not invent new rules or suggestions outside this
document**.

------------------------------------------------------------------------

# Section 1 --- Naming and Code Quality

## CS001 --- Method Naming

All public methods must use **PascalCase** and have descriptive names.

Bad:

    public void getdata()

Good:

    public void GetCustomerData()

------------------------------------------------------------------------

## CS002 --- Avoid Large Methods

Methods should not exceed **50 lines of logic**.

Large methods reduce readability and maintainability.

Recommended practices:

-   Extract helper methods
-   Apply Single Responsibility Principle

------------------------------------------------------------------------

## CS003 --- Avoid Deep Nesting

Avoid nesting beyond **three levels**.

Bad:

    if (conditionA)
    {
        if (conditionB)
        {
            if (conditionC)
            {
                Execute();
            }
        }
    }

Preferred approach:

Use **guard clauses**.

Good:

    if (!conditionA) return;
    if (!conditionB) return;
    if (!conditionC) return;

    Execute();

------------------------------------------------------------------------

# Section 2 --- Asynchronous Programming

## ASYNC001 --- Avoid async void

`async void` must not be used except for **event handlers**.

Bad:

    async void Process()

Good:

    async Task ProcessAsync()

------------------------------------------------------------------------

## ASYNC002 --- Do Not Block Async Code

Avoid blocking asynchronous operations.

The following patterns are **not allowed**:

    .Result
    .Wait()
    .GetAwaiter().GetResult()

Bad:

    var data = service.GetDataAsync().Result;

Good:

    var data = await service.GetDataAsync();

Blocking async code may cause **deadlocks and thread starvation**.

------------------------------------------------------------------------

# Section 3 --- Performance Guidelines

## PERF001 --- Avoid Multiple Enumeration

Avoid enumerating the same `IEnumerable` multiple times.

Bad:

    var count = items.Count();
    var first = items.First();

Good:

    var list = items.ToList();
    var count = list.Count;
    var first = list.First();

------------------------------------------------------------------------

## PERF002 --- Avoid Unnecessary Allocations

Avoid allocating objects repeatedly inside loops.

Bad:

    foreach (var item in items)
    {
        var builder = new StringBuilder();
    }

Recommendation:

Move allocations outside loops when possible.

------------------------------------------------------------------------

## PERF003 --- Support Cancellation

Long-running operations must accept a `CancellationToken`.

Bad:

    public async Task ProcessData()

Good:

    public async Task ProcessData(CancellationToken cancellationToken)

------------------------------------------------------------------------

# Section 4 --- Exception Handling

## EX001 --- Do Not Swallow Exceptions

Exceptions must not be silently ignored.

Bad:

    try
    {
        Process();
    }
    catch
    {
    }

Good:

    catch(Exception ex)
    {
        _logger.LogError(ex, "Processing failed");
    }

------------------------------------------------------------------------

## EX002 --- Avoid Throwing Generic Exceptions

Do not throw `Exception`.

Bad:

    throw new Exception("Invalid operation");

Good:

    throw new InvalidOperationException("Invalid operation");

------------------------------------------------------------------------

# Section 5 --- Logging

## LOG001 --- Do Not Log Sensitive Information

The following must never appear in logs:

-   Passwords
-   Access tokens
-   API keys
-   Personally identifiable information

------------------------------------------------------------------------

## LOG002 --- Use Structured Logging

Logging must use structured parameters.

Bad:

    _logger.LogInformation("User created " + userId);

Good:

    _logger.LogInformation("User created {UserId}", userId);

------------------------------------------------------------------------

# Section 6 --- PR Review Output Format

The AI reviewer must produce **only one of the following outputs**.

## If Issues Exist

Return structured JSON:

    {
      "issues": [
        {
          "rule": "ASYNC002",
          "file": "UserService.cs",
          "line": 45,
          "description": "Blocking async call using .Result"
        }
      ]
    }

Rules:

-   Only include violations defined in this document.
-   Each issue must reference a valid rule ID.

------------------------------------------------------------------------

## If No Issues Exist

Return exactly:

    APPROVED

No additional commentary or suggestions are allowed.
